import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateTemplateComponent } from './add-or-update/add-or-update-template.component';
import { TemplateRepository } from 'src/app/infrastructure/repositories/template.repository';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { TypeAttributeCode } from 'src/app/shared/utils/enums';
import { UserService } from 'src/app/core/services/user.service';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';

@Component({
    selector: 'app-template',
    templateUrl: './template.component.html'
})
export class TemplateComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    dataAll: any[] = [];
    loading = false;

    template_groups: any[] = [];

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã', index: 'Code' },
        { title: 'Tên biểu mẫu', index: 'Name' },
        { title: 'Nhóm biểu mẫu', render: 'template-group-column' },
        { title: 'File đính kèm', render: 'attactment-column' },
        { title: 'Ngày cập nhật', index: 'UpdatedAt', type: 'date', width: 120, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        { title: 'Ghi chú', index: 'Note' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit,
                    click: record => this.addOrUpdate(record)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá biểu mẫu này?',
                        okType: 'danger',
                        icon: 'star'
                    },
                    click: record => this.delete(record)
                }
            ]
        }
    ];

    constructor(
        private fb: FormBuilder,
        private modalSrv: NzModalService,
        private templateRepository: TemplateRepository,
        private message: NzMessageService,
        private drawerService: NzDrawerService,
        private typeAttributeRepository: TypeAttributeRepository,
        private userService: UserService,
        private uploadRepository: UploadRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getItemByTypeAttributeCode();
    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                this.getData();
                break;
            case 'dblClick':
                this.addOrUpdate(e.dblClick?.item);
                break;
            case 'checkbox':
                break;
            default:
                break;
        }
    }

    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
        this.getData();
    }

    searchData() {
        this.paging.page = 1;
        this.getData();
    }

    async getData() {
        this.paging.query = '1=1';
        this.paging.order_by = 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and Type=${this.query.type}`
        }

        try {
            this.loading = true;
            const resp = await this.templateRepository.getByPage(this.paging);

            if (resp.meta?.error_code == 200) {
                this.data = resp.data;
                this.paging.item_count = resp.metadata;
            } else {
                this.modalSrv.error({
                    nzTitle: 'Không lấy được dữ liệu.'
                });
            }
        } catch (error) {
            throw error;
        } finally {
            this.loading = false;
        }
    }

    addOrUpdate(record?: any): void {
        let template_groups = this.template_groups;

        const drawerRef = this.drawerService.create<AddOrUpdateTemplateComponent>({
            nzTitle: record ? `Sửa biểu mẫu: ${record.Name}` : 'Thêm mới biểu mẫu',
            // record.khoa_chinh
            nzWidth: '55vw',
            nzContent: AddOrUpdateTemplateComponent,
            nzContentParams: {
                record,
                template_groups
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa biểu mẫu ${data.Name} thành công!` : `Thêm mới biểu mẫu ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.templateRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa biểu mẫu ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    async getItemByTypeAttributeCode() {
        const resp = await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.NHOM_BIEU_MAU);

        if (resp.meta?.error_code == 200) {
            this.template_groups = resp.data;
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu nhóm biểu mẫu.'
            });
        }
    }

    findTemplateGroup(id: number) {
        let tplGroup = this.template_groups.find(x => x.Id == id);
        return tplGroup ? tplGroup.Name : undefined;
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}
