import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateDepartmentComponent } from './add-or-update/add-or-update-department.component';
import { DepartmentRepository } from 'src/app/infrastructure/repositories/department.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-department',
    templateUrl: './department.component.html'
})
export class DepartmentComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    dataAll: any[] = [];
    loading = false;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã', index: 'Code' },
        { title: 'Tên phòng ban', index: 'Name' },
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
                        title: 'Bạn có chắc chắn muốn xoá phòng ban này?',
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
        private departmentRepository: DepartmentRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
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
                this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
        }

        try {
            this.loading = true;
            const resp = await this.departmentRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateDepartmentComponent>({
            nzTitle: record ? `Sửa phòng ban: ${record.Name}` : 'Thêm mới phòng ban',
            // record.khoa_chinh
            nzWidth: '55vw',
            nzContent: AddOrUpdateDepartmentComponent,
            nzContentParams: {
                record
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa phòng ban ${data.Name} thành công!` : `Thêm mới phòng ban ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.departmentRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa phòng ban ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    import() {
        this.drawerService.create<SharedImportExcelComponent>({
            nzTitle: `Import excel danh sách phòng ban`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: SharedImportExcelComponent,
            nzContentParams: {
                importHistoryType: ImportHistoryTypeEnum.Common_Department
            }
        });
    }
}
