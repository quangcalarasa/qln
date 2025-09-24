import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateVerifycationUnitComponent } from './add-or-update/add-or-update-verifycation-unit.component';
import { VerifycationUnitRepository } from 'src/app/infrastructure/repositories/verifycation-unit.repository';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { TypeAttributeCode } from 'src/app/shared/utils/enums';
import { UserService } from 'src/app/core/services/user.service';

@Component({
    selector: 'app-verifycation-unit',
    templateUrl: './verifycation-unit.component.html'
})
export class VerifycationUnitComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    dataAll: any[] = [];
    loading = false;

    doc_typies: any[] = [];

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã cơ quan', index: 'Code' },
        { title: 'Tên cơ quan', index: 'Name' },
        { title: 'Loại văn bản xác minh', render: 'doc-type-column' },
        { title: 'Tình trạng xác minh', index: 'VerifyStatus', type: 'yn', safeType: "safeHtml", className: 'text-center' },
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
                        title: 'Bạn có chắc chắn muốn xoá cơ quan xác minh này?',
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
        private verifycationUnitRepository: VerifycationUnitRepository,
        private message: NzMessageService,
        private drawerService: NzDrawerService,
        private typeAttributeRepository: TypeAttributeRepository,
        private userService: UserService
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
            const resp = await this.verifycationUnitRepository.getByPage(this.paging);

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
        let doc_typies = this.doc_typies;

        const drawerRef = this.drawerService.create<AddOrUpdateVerifycationUnitComponent>({
            nzTitle: record ? `Sửa cơ quan xác minh: ${record.Name}` : 'Thêm mới cơ quan xác minh',
            // record.khoa_chinh
            nzWidth: '55vw',
            nzContent: AddOrUpdateVerifycationUnitComponent,
            nzContentParams: {
                record,
                doc_typies
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa cơ quan xác minh ${data.Name} thành công!` : `Thêm mới cơ quan xác minh ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.verifycationUnitRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa cơ quan xác minh ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    async getItemByTypeAttributeCode() {
        const resp = await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.LOAI_VAN_BAN_XAC_MINH);

        if (resp.meta?.error_code == 200) {
            this.doc_typies = resp.data;
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu loại văn bản xác minh.'
            });
        }
    }

    findDocType(id: number) {
        let tplGroup = this.doc_typies.find(x => x.Id == id);
        return tplGroup ? tplGroup.Name : undefined;
    }
}
