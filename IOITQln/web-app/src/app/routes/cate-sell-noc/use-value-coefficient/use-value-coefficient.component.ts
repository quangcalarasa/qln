import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateUseValueCoefficientComponent } from './add-or-update/add-or-update-use-value-coefficient.component';
// import { ConstructionPriceRepository } from 'src/app/infrastructure/repositories/construction-price.repository';
import { UseValueCoefficientRepository } from 'src/app/infrastructure/repositories/use-value-coefficient.repository';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { AccessKey, TypeDecree } from 'src/app/shared/utils/enums';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
import { Decree } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-use-value-coefficient',
    templateUrl: './use-value-coefficient.component.html'
})
export class UseValueCoefficientComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    // decree_type1_data = [];
    decree_type2_data = [];
    floor_data: any[] = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.USE_VALUE_COEFFICIENT_MANAGEMENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Nghị định áp dụng cho biên bản', index: 'DecreeType1Id', width: 150, type: 'enum', enum: Decree },
        { title: 'Văn bản pháp luật liên quan', index: 'DecreeType2Name', width: 150 },
        { title: 'Loại biên bản áp dụng', render: 'typeReportApply-column', width: 150 },
        { title: 'Bảng hệ số điều chỉnh', render: 'detail-column' },
        { title: 'Căn cứ', index: 'Des' },
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
                        title: 'Bạn có chắc chắn muốn xoá hệ số điều chỉnh giá trị sử dụng?',
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
        // private constructionPriceRepository: ConstructionPriceRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private useValueCoefficientRepository: UseValueCoefficientRepository,
        private decreeRepository: DecreeRepository,
        private floorRepository: FloorRepository,
        private commonService: CommonService,
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        // this.getDataDecreeType1();
        this.getDataDecreeType2();
        this.getDataFloor();
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
                this.paging.query += ` and Des.Contains("${this.query.txtSearch}")`;
        }

        try {
            this.loading = true;
            const resp = await this.useValueCoefficientRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateUseValueCoefficientComponent>({
            nzTitle: record ? `Sửa hệ số điều chỉnh giá trị sử dụng` : 'Thêm mới hệ số điều chỉnh giá trị sử dụng',
            // record.khoa_chinh
            nzWidth: '75vw',
            nzContent: AddOrUpdateUseValueCoefficientComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                // decree_type1_data: this.decree_type1_data,
                decree_type2_data: this.decree_type2_data,
                floor_data: this.floor_data
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa hệ số điều chỉnh giá trị sử dụng thành công!` : `Thêm mới hệ số điều chỉnh giá trị sử dụng thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.useValueCoefficientRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa hệ số điều chỉnh giá trị sử dụng thành công!`);
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

    async getDataFloor() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = '1=1';
        paging.select = "Id,Code,Name";

        const resp = await this.floorRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.floor_data = resp.data;
        }
    }

    genFloorName(id: number) {
        let floor = this.floor_data.find(x => x.Id == id);

        return floor ? floor.Name : "";
    }

    genUseValueCoefficientTypeReportApply(typeReportApply: number) {
        return TypeReportApply[typeReportApply as unknown as keyof typeof TypeReportApply];
    }
}
