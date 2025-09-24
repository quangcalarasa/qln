import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { Md167ReceiptRepository } from 'src/app/infrastructure/repositories/md167-receipt.repository';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AddOrUpdateReceiptComponent } from '../add-or-update-receipt/add-or-update-receipt.component';
import { NzMessageService } from 'ng-zorro-antd/message';
import { EventMd167ReceiptService } from 'src/app/core/services/event-md167Receipt.service';

@Component({
    selector: 'app-add-or-update-md167-receipt',
    templateUrl: './receipt.component.html'
})
export class Md167ReceiptComponent implements OnInit {

    @Input() record: any;
    data: any;

    loading: boolean = false;

    constructor(
        private fb: FormBuilder,
        private cdr: ChangeDetectorRef,
        private md167ReceiptRepository: Md167ReceiptRepository,
        private modal: NzModalRef,
        private modalSrv: NzModalService,
        private message: NzMessageService,
        private eventMd167ReceiptService: EventMd167ReceiptService
    ) { }

    ngOnInit(): void {
        this.getData();
    }

    close(): void {
        this.modal.close();
    }

    //get danh sách thanh toán của hợp đồng
    async getData() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = `Md167ContractId=${this.record.Id}`;
        paging.order_by = "DateOfPayment Desc";

        const resp = await this.md167ReceiptRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data = resp.data;
        }
    }

    addOrUpdate(record?: any) {
        this.modalSrv.create({
            nzTitle: record ? `Sửa phiếu thanh toán "${record.ReceiptCode}"` : `Thêm phiếu thanh toán`,
            nzContent: AddOrUpdateReceiptComponent,
            nzWidth: '40vw',
            nzComponentParams: {
                record: record,
                contract: this.record
            },
            nzOnOk: (res: any) => {
                this.getData();
                this.eventMd167ReceiptService.sendMessage();
            }
        });
    }

    confirmDetele(record: any) {
        this.modalSrv.confirm({
            nzTitle: 'Xác nhận xóa phiếu thanh toán ' + record.ReceiptCode + ' ?',
            nzOnOk: () => {
                this.delete(record);
            },
            nzOkText: "Xác nhận",
            nzCancelText: 'Đóng'
        });
    }

    async delete(data: any) {
        const resp = await this.md167ReceiptRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa phiếu thanh toán ${data.ReceiptCode} thành công!`);
            this.getData();
        }
    }
}
