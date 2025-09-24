import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { SettingsService } from '@delon/theme';
import { PromissoryRepository } from '../../../../infrastructure/repositories/Promissory.repository';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { STColumn } from '@delon/abc/st';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.component.html',
  styles: []
})
export class PaymentComponent implements OnInit {
  @Input() code: string;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data: any;
  validateForm!: FormGroup;
  loading: boolean = false;
  newDate = convertDate(new Date().toString());

  columns: STColumn[] = [
    { title: 'STT', type: 'no', width: 40 },
    { title: 'STT phiếu', index: 'Number'},
    { title: 'Mã định danh', index: 'Code' },
    { title: 'Ngày thực hiện', index: 'Date', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Nội dung thu', index: 'Note' },
    { renderTitle: 'PriceHeader', render: 'Price', className: 'text-right' },
    { title: 'Số phiếu chuyển', index: 'NumberOfTransfer' },
    { title: 'Ngày phiếu chuyển', index: 'DateOfTransfer', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Mã xuất hóa đơn', index: 'InvoiceCode' }
  ];

  constructor(
    private fb: FormBuilder,
    private message: NzMessageService,
    private modal: NzModalRef,
    private settings: SettingsService,
    private promissoryRepository: PromissoryRepository
  ) {}

  get user(): any {
    return this.settings.user;
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Code: [this.code],
      Date: [new Date().toISOString().slice(0, 10), [Validators.required]],
      Executor: [this.user.FullName, [Validators.required]],
      Price: [undefined, [Validators.required]],
      Action: [1], //1 là thu - 2 là chi,
      NumberOfTransfer: [undefined, Validators.required],
      InvoiceCode: [undefined, Validators.required],
      DateOfTransfer: [undefined, Validators.required],
      Number: [undefined, Validators.required],
      Note: [undefined]
    });
    this.getData();
  }

  async submitForm() {
    let data = { ...this.validateForm.value };
    try {
      this.loading = true;
      const resp = await this.promissoryRepository.addNew(data);

      if (resp.meta?.error_code == 200) {
        this.modal.triggerOk();
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  close(): void {
    this.modal.close();
  }

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `Action=${1} AND  Code.Contains("${this.code}")`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.promissoryRepository.getByPage(this.paging);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }
}
