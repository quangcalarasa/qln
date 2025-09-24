import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateCurrentStateMainTextureComponent } from './add-or-update/add-or-update-currstate-maintexture.component';
import { CurrentStateMainTextureRepository } from 'src/app/infrastructure/repositories/ct-maintexture.repository';
import { TypeMainTexTure, LevelBlock } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-currstate-maintexture',
    templateUrl: './currstate-maintexture.component.html'
})
export class CurrentStateMainTextureComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;
    role = this.commonService.CheckAccessKeyRole(AccessKey.CURRENTSTATE_MAINTEXTURE_MANAGEMENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Khai báo bảng tỷ lệ chất lượng còn lại cho cấp của căn nhà', index: 'LevelBlock', type: 'enum', enum: LevelBlock },
        { title: 'Kết cấu chính', index: 'TypeMainTexTure', type: 'enum', enum: TypeMainTexTure },
        { title: 'Phân hạng chi tiết theo kiến trúc', index: 'Name' },
        { title: 'Mặc định', index: 'Default', type: 'yn', safeType: "safeHtml", className: "text-center" },
        { title: 'Ghi chú', index: 'Note' },
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
                        title: 'Bạn có chắc chắn muốn xoá Hiện trạng kết cấu chính này?',
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
        private currentStateMainTextureRepository: CurrentStateMainTextureRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private commonService: CommonService,
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
                this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Note.Contains("${this.query.txtSearch}"))`;
        }

        try {
            this.loading = true;
            const resp = await this.currentStateMainTextureRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateCurrentStateMainTextureComponent>({
            nzTitle: record ? `Sửa Hiện trạng kết cấu chính: ${record.Name}` : 'Thêm mới  Hiện trạng kết cấu chính',
            // record.khoa_chinh
            nzWidth: '35vw',
            nzContent: AddOrUpdateCurrentStateMainTextureComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa Hiện trạng kết cấu chính ${data.Name} thành công!` : `Thêm mới Hiện trạng kết cấu chính ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.currentStateMainTextureRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa Hiện trạng kết cấu chính ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }
}
