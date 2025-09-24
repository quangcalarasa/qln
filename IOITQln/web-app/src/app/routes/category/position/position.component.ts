import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdatePositionComponent } from './add-or-update/add-or-update-position.component';
import { PositionRepository } from 'src/app/infrastructure/repositories/position.repository';
import { DepartmentRepository } from 'src/app/infrastructure/repositories/department.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-position',
    templateUrl: './position.component.html'
})
export class PositionComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    dataAll: any[] = [];
    loading = false;

    departments: any[] = [];

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã chức danh', index: 'Code' },
        { title: 'Tên chức danh', index: 'Name' },
        { title: 'Phòng ban', render: 'department-column' },
        { title: 'Mã VB ủy quyền', index: 'AuthorDocsCode' },
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
                        title: 'Bạn có chắc chắn muốn xoá chức danh này?',
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
        private positionRepository: PositionRepository,
        private message: NzMessageService,
        private drawerService: NzDrawerService,
        private departmentRepository: DepartmentRepository,
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDepartments();
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
            this.paging.query += ` and DepartmentId=${this.query.type}`
        }

        try {
            this.loading = true;
            const resp = await this.positionRepository.getByPage(this.paging);

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
        let departments = this.departments;

        const drawerRef = this.drawerService.create<AddOrUpdatePositionComponent>({
            nzTitle: record ? `Sửa chức danh: ${record.Name}` : 'Thêm mới chức danh',
            nzWidth: '55vw',
            nzContent: AddOrUpdatePositionComponent,
            nzContentParams: {
                record,
                departments
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa chức danh ${data.Name} thành công!` : `Thêm mới chức danh ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.positionRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa chức danh ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    async getDepartments() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;

        const resp = await this.departmentRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.departments = resp.data;
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu phòng ban.'
            });
        }
    }

    findDepartment(id: number) {
        let tplGroup = this.departments.find(x => x.Id == id);
        return tplGroup ? tplGroup.Name : undefined;
    }

    import() {
        this.drawerService.create<SharedImportExcelComponent>({
            nzTitle: `Import excel danh sách chức vụ`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: SharedImportExcelComponent,
            nzContentParams: {
                importHistoryType: ImportHistoryTypeEnum.Common_Position
            }
        });
    }
}
