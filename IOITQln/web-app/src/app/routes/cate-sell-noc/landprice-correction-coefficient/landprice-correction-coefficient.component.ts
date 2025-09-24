import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateLandPriceCorrectionCoefficientComponent } from './add-or-update/add-or-update-landprice-correction-coefficient.component';
import { LandPriceCorrectionCoefficientRepository } from 'src/app/infrastructure/repositories/landprice-correction-coefficient.repository';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { AccessKey, TypeDecree } from 'src/app/shared/utils/enums';
import { Decree } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-landprice-correction-coefficient',
    templateUrl: './landprice-correction-coefficient.component.html'
})
export class LandPriceCorrectionCoefficientComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();
    role = this.commonService.CheckAccessKeyRole(AccessKey.LAND_PRICE_CORRECTION_COEFFICIENT_MANAGEMENT);
    data: any[] = [];
    loading = false;

    // decree_type1_data = [];
    decree_type2_data = [];

    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Nghị định áp dụng cho biên bản', index: 'DecreeType1Id', type: 'enum', enum: Decree },
        { title: 'Văn bản pháp luật liên quan', index: 'DecreeType2Name' },
        { title: 'Hệ số K điều chỉnh giá đất', index: 'Name' },
        { title: 'Diễn giải', index: 'Note' },
        { title: 'Giá trị', render: 'valueClmn', className: 'text-center' },
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
                        title: 'Bạn có chắc chắn muốn xoá thông tin Hệ số K này?',
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
        private landPriceCorrectionCoefficientRepository: LandPriceCorrectionCoefficientRepository,
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

        try {
            this.loading = true;
            const resp = await this.landPriceCorrectionCoefficientRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateLandPriceCorrectionCoefficientComponent>({
            nzTitle: record ? `Sửa thông tin Hệ số K` : 'Thêm mới thông tin Hệ số K',
            // record.khoa_chinh
            nzWidth: '75vw',
            nzContent: AddOrUpdateLandPriceCorrectionCoefficientComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                // decree_type1_data: this.decree_type1_data,
                decree_type2_data: this.decree_type2_data
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa thông tin Hệ số K thành công!` : `Thêm mới thông tin Hệ só K thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.landPriceCorrectionCoefficientRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa thông tin Hệ số K thành công!`);
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
