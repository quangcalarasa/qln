import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateDecreeComponent } from './add-or-update/add-or-update-decree.component';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { AccessKey, TypeDecree } from 'src/app/shared/utils/enums';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-decree',
    templateUrl: './decree.component.html'
})
export class DecreeComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    typeDecree: number = TypeDecree.NGHIDINH;
    typeDecreeString?: string;
    role = this.commonService.CheckAccessKeyRole(AccessKey.DECREE_TYPE2_MANAGEMENT);
    columns: STColumn[];

    constructor(
        private fb: FormBuilder,
        private modalSrv: NzModalService,
        private decreeRepository: DecreeRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private router: Router,
        private commonService: CommonService,
    ) {
        const curentUrl = this.router.url;
        if (curentUrl.indexOf("decree-type1") !== -1) {
            this.typeDecree = TypeDecree.NGHIDINH;
            this.typeDecreeString = "Nghị định";
        }
        else if (curentUrl.indexOf("decree-type2") !== -1) {
            this.typeDecree = TypeDecree.THONGTU;
            this.typeDecreeString = "Văn bản pháp luật liên quan";
        }

        this.columns = [
            { title: 'Stt', type: 'no', width: 40 },
            { title: this.typeDecree == TypeDecree.NGHIDINH ? 'Nghị định số' : 'Văn bản số', index: 'Code' },
            { title: 'Ngày ban hành', index: 'DoPub', type: 'date', className: 'text-center', dateFormat: 'dd/MM/yyyy' },
            { title: 'Cơ quan ban hành', index: 'DecisionUnit' },
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
                        iif: i=>this.role.Delete,
                        pop: {
                            title: `Bạn có chắc chắn muốn xoá ${this.typeDecreeString} này?`,
                            okType: 'danger',
                            icon: 'star'
                        },
                        click: record => this.delete(record)
                    }
                ]
            }
        ];
    }

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
        this.paging.query = `TypeDecree=${this.typeDecree}`;
        this.paging.order_by = 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or DecisionUnit.Contains("${this.query.txtSearch}") or Note.Contains("${this.query.txtSearch}"))`;
        }

        try {
            this.loading = true;
            const resp = await this.decreeRepository.getByPage(this.paging, this.typeDecree);

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
        const drawerRef = this.drawerService.create<AddOrUpdateDecreeComponent>({
            nzTitle: record ? `Sửa ${this.typeDecreeString} : ${record.Code}` : `Thêm mới ${this.typeDecreeString}`,
            // record.khoa_chinh
            nzWidth: '60vw',
            nzContent: AddOrUpdateDecreeComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                typeDecree: this.typeDecree
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa ${this.typeDecreeString} ${data.Code} thành công!` : `Thêm mới ${this.typeDecreeString} ${data.Code} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.decreeRepository.delete(data, this.typeDecree);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa ${this.typeDecreeString} ${data.Code} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }
}
