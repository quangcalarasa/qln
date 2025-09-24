import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateManualDocumentComponent } from './add-or-update/add-or-update-manual-document.component';
import { ManualDocumentRepository } from 'src/app/infrastructure/repositories/manual-document.repository';
import { ModuleSystem } from 'src/app/shared/utils/consts';
import { UserService } from 'src/app/core/services/user.service';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';

@Component({
    selector: 'app-template',
    templateUrl: './manual-document.component.html'
})
export class ManualDocumentComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    dataModuleSystem = ModuleSystem;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Tên', index: 'Title' },
        { title: 'Loại', render: 'showModuleSystem-column'},
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
                        title: 'Bạn có chắc chắn muốn xoá bản ghi này?',
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
        private manualDocumentRepository: ManualDocumentRepository,
        private message: NzMessageService,
        private drawerService: NzDrawerService,
        private userService: UserService,
        private uploadRepository: UploadRepository
    ) { }

    ngOnInit(): void {
        this.getData();
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
                this.paging.query += ` and (Title.Contains("${this.query.txtSearch}")` + ` or Note.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and Type=${this.query.type}`
        }

        try {
            this.loading = true;
            const resp = await this.manualDocumentRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateManualDocumentComponent>({
            nzTitle: record ? `Sửa hướng dẫn sử dung: ${record.Name}` : 'Thêm mới hướng dẫn sử dụng',
            // record.khoa_chinh
            nzWidth: '55vw',
            nzContent: AddOrUpdateManualDocumentComponent,
            nzContentParams: {
                record,
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa hướng dẫn sử dụng ${data.Name} thành công!` : `Thêm mới hướng dẫn sử dụng ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.manualDocumentRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa hướng dẫn sử dụng ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}
