import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateVatComponent } from './add-or-update/add-or-update-vat.component';
import { VatRepository } from 'src/app/infrastructure/repositories/vat.repository';

@Component({
    selector: 'app-cate-vat',
    templateUrl: './vat.component.html'
})
export class VatComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Ngày áp dụng', index: 'DoApply', type: 'date', className: 'text-center', width: 150, dateFormat: 'dd/MM/yyyy' },
        { title: 'Giá trị', render: 'vClmn', className: 'text-center', width: 150 },
        { title: 'Diễn giải', index: 'Note' },
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
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn hệ số VAT này?',
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
        private vatRepository: VatRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService
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
                this.paging.query += ` and Note.Contains("${this.query.txtSearch}")`;
        }

        try {
            this.loading = true;
            const resp = await this.vatRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateVatComponent>({
            nzTitle: record ? `Sửa hệ số VAT` : 'Thêm mới hệ số VAT',
            // record.khoa_chinh
            nzWidth: '35vw',
            nzContent: AddOrUpdateVatComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa hệ số VAT thành công!` : `Thêm hệ số VAT thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.vatRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa hệ số lương cơ bản thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }
}
