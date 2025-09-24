import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateDistrictComponent } from './add-or-update/add-or-update-district.component';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { Router } from '@angular/router';

@Component({
    selector: 'app-district',
    templateUrl: './district.component.html'
})
export class DistrictComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    data_province: any[] = [];
    dataAll: any[] = [];
    loading = false;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã', index: 'Code' },
        { title: 'Tên quận/huyện', index: 'Name' },
        { title: 'Tỉnh/thành phố', render: 'province-column' },
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
                        title: 'Bạn có chắc chắn muốn xoá quận/huyện này?',
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
        private provinceRepository: ProvinceRepository,
        private districtRepository: DistrictRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDataProvince();
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
        if(this.router.url=="/cate-md167/md167-location/md167-district")
            return true
        return false
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

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and ProvinceId=${this.query.type}`
        }

        try {
            this.loading = true;
            const resp = await this.districtRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateDistrictComponent>({
            nzTitle: record ? (updateName==true?`Sửa tên quận/huyện: ${record.Name}`:`Sửa quận/huyện: ${record.Name}`) : 'Thêm mới quận/huyện',
            nzWidth: '55vw',
            nzContent: AddOrUpdateDistrictComponent,
            nzContentParams: {
                record,
                data_province,
                isUpdateName:updateName
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa quận/huyện ${data.Name} thành công!` : `Thêm mới quận/huyện ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.districtRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa quận/huyện ${data.Name} thành công!`);
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
}
