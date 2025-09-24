import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateLaneComponent } from './add-or-update/add-or-update-lane.component';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { Router } from '@angular/router';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';

@Component({
    selector: 'app-cate-lane',
    templateUrl: './lane.component.html'
})
export class LaneComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    data_province: any[] = [];
    data_district: any[] = [];
    data_district_filter: any[] = [];
    data_ward: any[] = [];
    data_ward_filter: any[] = [];
    loading = false;

    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Tên đường', index: 'Name' },
        { title: 'Chi tiết', index: 'FullAddress' },
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
                    icon: 'form',
                    iif:i=>this.compareUrl(),
                    click: record => this.addOrUpdate(record,true)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá đường?',
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
        private laneRepository: LaneRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private router: Router,
        private provinceRepository: ProvinceRepository,
        private districtRepository: DistrictRepository,
        private wardRepository: WardRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDataProvince();
        this.getDataDistrict();
        this.getDataWard();
    }
    compareUrl()
    {
        if(this.router.url=="/cate-md167/md167-location/md167-lane")
            return true
        return false
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
            this.paging.query += ` and Province=${this.query.type}`;
        }

        if (this.query.type1 != undefined) {
            this.paging.query += ` and District=${this.query.type1}`;
        }

        if (this.query.type2 != undefined) {
            this.paging.query += ` and Ward=${this.query.type2}`;
        }

        try {
            this.loading = true;
            const resp = await this.laneRepository.getByPage(this.paging);

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

    addOrUpdate(record?: any , updateName?:any): void {
        const drawerRef = this.drawerService.create<AddOrUpdateLaneComponent>({
            nzTitle: record ? ( updateName==true?`Sửa tên đường: ${record.Name}`:`Sửa đường: ${record.Name}` ): 'Thêm mới đường',
            // record.khoa_chinh
            nzWidth: '40vw',
            nzContent: AddOrUpdateLaneComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                isUpdateName:updateName
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa đường ${data.Name} thành công!` : `Thêm mới đường ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.laneRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa đường ${data.Name} thành công!`);
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
            nzTitle: `Import excel danh sách đường`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: SharedImportExcelComponent,
            nzContentParams: {
                importHistoryType: ImportHistoryTypeEnum.Common_Lane
            }
        });
    }

    async getDataProvince() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Code,Name";

        const resp = await this.provinceRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_province = resp.data;
        }
    }

    async getDataDistrict() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Code,Name,ProvinceId";

        const resp = await this.districtRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_district = resp.data;
        }
    }

    async getDataWard() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Code,Name,DistrictId,ProvinceId";

        const resp = await this.wardRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_ward = resp.data;
        }
    }

    districtByProvinceId() {
        this.data_district_filter = [];

        if (this.query.type) {
            this.data_district_filter = this.data_district.filter(x => x.ProvinceId == this.query.type);
        }
    }

    wardByDistrictId() {
        this.data_ward_filter = [];

        if (this.query.type1) {
            this.data_ward_filter = this.data_ward.filter(x => x.DistrictId == this.query.type1);
        }
    }
}
