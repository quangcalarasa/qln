import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateAreaComponent } from './add-or-update/add-or-update-area.component';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-area',
    templateUrl: './area.component.html'
})
export class AreaComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    data_floor: any[] = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.AREA_MANAGEMENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Tầng của căn nhà', index: 'FloorName' },
        { title: 'Thông tin chi tiết tầng cụ thể', index: 'Name' },
        { title: 'Tầng lửng', index: 'IsMezzanine', type: 'yn', safeType: "safeHtml", className: "text-center" },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit && this.role.Update,
                    click: record => this.addOrUpdate(record)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    iif: i => this.role.Delete,
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá Thông tin chi tiết tầng cụ thể này?',
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
        private areaRepository: AreaRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private floorRepository: FloorRepository,
        private commonService: CommonService,
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDataFloor();
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
            const resp = await this.areaRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateAreaComponent>({
            nzTitle: record ? `Sửa Thông tin chi tiết tầng cụ thể: ${record.Name}` : 'Thêm mới Thông tin chi tiết tầng cụ thể',
            // record.khoa_chinh
            nzWidth: '40vw',
            nzContent: AddOrUpdateAreaComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                data_floor: this.data_floor
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa Thông tin chi tiết tầng cụ thể ${data.Name} thành công!` : `Thêm mới Thông tin chi tiết tầng cụ thể ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.areaRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa Thông tin chi tiết tầng cụ thể ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    async getDataFloor() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Name";

        const resp = await this.floorRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_floor = resp.data;
        }
    }
}
