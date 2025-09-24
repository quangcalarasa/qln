import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateLandSpecialCoefficientComponent } from './add-or-update/add-or-update-land-special-coefficient.component';
import { LandSpecialCoefficientRepository } from 'src/app/infrastructure/repositories/land-special-coefficient.repository';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { AccessKey, TypeDecree } from 'src/app/shared/utils/enums';
import { Decree } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-land-special-coefficient',
    templateUrl: './land-special-coefficient.component.html'
})
export class LandSpecialCoefficientComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    // decree_type1_data = [];
    decree_type2_data = [];

    Decree = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.LAND_SPECIAL_COEFFICIENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Nghị định áp dụng cho biên bản', index: 'DecreeType1Id', type: 'enum', enum: Decree },
        { title: 'Văn bản pháp luật liên quan', index: 'DecreeType2Name' },
        { title: 'Ngày áp dụng', index: 'DoApply', type: 'date', width: 150, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        { title: 'Có phần diện tích không mặt tiền đường (hẻm) từ 15m2 trở lên', render: 'value1Clmn', className: 'text-center' },
        { title: '5R< D <=8R', render: 'value2Clmn', className: 'text-center' },
        { title: 'D > 8R', render: 'value3Clmn', className: 'text-center' },
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
                        title: 'Bạn có chắc chắn muốn xoá thông tin Hệ số này?',
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
        private landSpecialCoefficientRepository: LandSpecialCoefficientRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private decreeRepository: DecreeRepository,
        private commonService: CommonService,
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        // this.getDataDecreeType1();
        this.getDataDecreeType2();
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
                this.paging.query += ` and Name.Contains("${this.query.txtSearch}")`;
        }

        if (this.query.type1 != undefined) {
            this.paging.query += ` and DecreeType1Id=${this.query.type1}`
        }

        try {
            this.loading = true;
            const resp = await this.landSpecialCoefficientRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateLandSpecialCoefficientComponent>({
            nzTitle: record ? `Sửa thông tin Hệ số` : 'Thêm mới thông tin Hệ số',
            // record.khoa_chinh
            nzWidth: '80vw',
            nzContent: AddOrUpdateLandSpecialCoefficientComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                // decree_type1_data: this.decree_type1_data,
                decree_type2_data: this.decree_type2_data
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa thông tin Hệ số thành công!` : `Thêm mới thông tin Hệ số thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.landSpecialCoefficientRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa thông tin Hệ số thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    // async getDataDecreeType1() {
    //     let paging: GetByPageModel = new GetByPageModel();
    //     paging.page_size = 0;
    //     paging.query = `TypeDecree=${TypeDecree.NGHIDINH}`;
    //     paging.select = "Id,Code";

    //     const resp = await this.decreeRepository.getByPage(paging, TypeDecree.NGHIDINH);

    //     if (resp.meta?.error_code == 200) {
    //         this.decree_type1_data = resp.data;
    //     }
    // }

    async getDataDecreeType2() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = `TypeDecree=${TypeDecree.THONGTU}`;
        paging.select = "Id,Code";

        const resp = await this.decreeRepository.getByPage(paging, TypeDecree.THONGTU);

        if (resp.meta?.error_code == 200) {
            this.decree_type2_data = resp.data;
        }
    }
}
