import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateLandscapeLimitComponent } from './add-or-update/add-or-update-landscape-limit.component';
import { LandscapeLimitRepository } from 'src/app/infrastructure/repositories/landscape-limit.repository';
import { AccessKey, TypeDecree } from 'src/app/shared/utils/enums';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { Decree } from 'src/app/shared/utils/consts';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-landscape-limit',
    templateUrl: './landscape-limit.component.html'
})
export class LandscapeLimitComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    decree_type2_data = [];
    province_data = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.LANDSCAPE_LIMIT_MANAGEMENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Nghị định', index: 'DecreeType1Id', width: 200, type: 'enum', enum: Decree },
        { title: 'Văn bản pháp luật liên quan', render: 'decreeTypeName-column' },
        { title: 'Loại biên bản áp dụng', index: 'TypeReportApply', width: 200, type: 'enum', enum: TypeReportApply },
        { title: 'Tỉnh/tp áp dụng', render: 'province-column' },
        { title: 'Diễn giải', index: 'Note' },
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
                        title: 'Bạn có chắc chắn muốn xoá Hạn mức đất ở này?',
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
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private landscapeLimitRepository: LandscapeLimitRepository,
        private decreeRepository: DecreeRepository,
        private provinceRepository: ProvinceRepository,
        private commonService: CommonService,
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getDataDecreeType2();
        this.getProvinceData();
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
                this.paging.query += ` and (Note.Contains("${this.query.txtSearch}") OR Note.Contains("${this.query.txtSearch}"))`;
        }

        try {
            this.loading = true;
            const resp = await this.landscapeLimitRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateLandscapeLimitComponent>({
            nzTitle: record ? `Sửa thông tin Hạn mức đất ở` : 'Thêm mới thông tin Hạn mức đất ở',
            // record.khoa_chinh
            nzWidth: '60vw',
            nzContent: AddOrUpdateLandscapeLimitComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                decree_type2_data: this.decree_type2_data,
                province_data: this.province_data
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa thông tin Hạn mức đất ở thành công!` : `Thêm mới thông tin Hạn mức đất ở thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.landscapeLimitRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa thông tin Hạn mức đất ở thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

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

    async getProvinceData() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.order_by = "Id Asc";
        paging.page_size = 0;
        paging.select = "Id,Code,Name";

        const resp = await this.provinceRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.province_data = resp.data;
        }
    }
}
