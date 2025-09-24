import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateWardComponent } from './add-or-update/add-or-update-ward.component';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { Router } from '@angular/router';

@Component({
    selector: 'app-ward',
    templateUrl: './ward.component.html'
})
export class WardComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    data_province: any[] = [];
    data_district: any[] = [];
    data_district_filter: any[] = [];
    dataAll: any[] = [];
    loading = false;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã', index: 'Code' },
        { title: 'Tên phường/xã', index: 'Name' },
        { title: 'Tỉnh/thành phố', render: 'province-column' },
        { title: 'Quận/huyện', render: 'district-column' },
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
                    icon: 'form',
                    iif:i=>this.compareUrl(),
                    click: record => this.addOrUpdate(record,true)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá phường/xã này?',
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
        private router: Router,
        private modalSrv: NzModalService,
        private provinceRepository: ProvinceRepository,
        private districtRepository: DistrictRepository,
        private wardRepository: WardRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDataProvince();
        this.getDataDistrict();
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
    compareUrl()
    {
        if(this.router.url=="/cate-md167/md167-location/md167-ward")
            return true
        return false
    }
    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
        this.getData();
        this.getDataProvince();
        this.getDataDistrict();
    }

    searchData() {
        this.paging.page = 1;
        this.getData();
    }

    async getData() {
        this.paging.query = '1=1';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and ProvinceId=${this.query.type}`
        }

        if (this.query.type1 != undefined) {
            this.paging.query += ` and DistrictId=${this.query.type1}`
        }

        try {
            this.loading = true;
            const resp = await this.wardRepository.getByPage(this.paging);

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

    addOrUpdate(record?: any, updateName?:any): void {
        let data_province = this.data_province;
        let data_district = this.data_district;

        const drawerRef = this.drawerService.create<AddOrUpdateWardComponent>({
            nzTitle: record ?  (updateName==true?`Sửa tên phường/xã: ${record.Name}`:`Sửa phường/xã: ${record.Name}`) : 'Thêm mới phường/xã',
            nzWidth: '55vw',
            nzContent: AddOrUpdateWardComponent,
            nzContentParams: {
                record,
                data_province,
                data_district,
                isUpdateName:updateName
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa phường/xã ${data.Name} thành công!` : `Thêm mới phường/xã ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.wardRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa phường/xã ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    async getDataProvince() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Code,Name";

        const resp = await this.provinceRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_province = resp.data;
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu phòng ban.'
            });
        }
    }

    findProvince(id: number) {
        let item = this.data_province.find(x => x.Id == id);
        return item ? item.Name : undefined;
    }

    async getDataDistrict() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = "Id,Code,Name,ProvinceId";

        const resp = await this.districtRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_district = resp.data;
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu phòng ban.'
            });
        }
    }

    findDistrict(id: number) {
        let item = this.data_district.find(x => x.Id == id);
        return item ? item.Name : undefined;
    }

    districtByProvinceId() {
        this.data_district_filter = [];

        if (this.query.type) {
            this.data_district_filter = this.data_district.filter(x => x.ProvinceId == this.query.type);
        }
    }
}
