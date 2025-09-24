import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { TdcPriceRentPayRepository } from 'src/app/infrastructure/repositories/tdcPriceRentPay.repositories';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';
import { convertMoney } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-pay-tdc-rent-component',
  templateUrl: './pay-tdc-rent-component.component.html'
})
export class PayTdcRentComponentComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data: any[] = [];
  dataTable: any[] = [];
  data_PayCount: any[] = [];
  totalPay: any;
  newDate = convertDate(new Date().toString());
  valueMax: any;
  valueMin: any;
  payCount: number;

  columns: STColumn[] = [
    { title: 'STT', type: 'no', width: 40 },
    { title: 'Ngày Thanh Toán', index: 'PaymentDate', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { renderTitle: 'AmountPaidHeader', render: 'AmountPaid' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xóa đợt thanh toán này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];
  constructor(
    private tdcPriceRentPayRepository: TdcPriceRentPayRepository,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
    private message: NzMessageService,
    private tdcPriceRentRepository: TdcPriceRentRepository,
    private modalSrv: NzModalService
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      PaymentDate: [new Date().toISOString().slice(0, 10)],
      AmountPaid: [this.record ? convertMoney(this.record.AmountPaid) : undefined],
      PayTime: [this.record ? this.record.PayTime : undefined],
      TdcPriceRentId: [this.record ? this.record.Id : undefined],
      PayCount: [this.record ? this.record.PayCount : undefined],
      PricePublic: [this.record ? this.record.PricePublic : undefined]
    });
    this.getData();
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.Value = this.validateForm.value.Value;
    data.PayCount = this.payCount;
    const resp = await this.tdcPriceRentPayRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `TdcPriceRentId=${this.record.Id}`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.tdcPriceRentPayRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
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

  async delete(data: any) {
    const resp = await this.tdcPriceRentPayRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa đợt thanh toán thành công!`);
      this.close();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  //Lấy data
  async getExcelTable(date: any) {
    const resp = await this.tdcPriceRentRepository.getExcelTable(this.record.Id, date);
    if (resp.meta?.error_code == 200) {
      this.dataTable = resp.data.filter((i: any) => i.Status === false || (i.Status === false && i.tdcPriceRentExcels[0].TypeRow === 4));
    }

    for (let i = 0; i < this.dataTable.length; i++) {
      for (let j = 0; j < this.dataTable[i].tdcPriceRentExcels.length; j++) {
        if (this.dataTable[i].tdcPriceRentExcels[j].TypeRow == 2) {
          this.dataTable[i].Pay = 0;
        }
      }
      if (this.dataTable[i].Pay != null) {
        if (i > 0) {
          this.dataTable[i].Pay = Math.round(this.dataTable[i].Pay + this.dataTable[i - 1].Pay);
        }
      }
    }
    this.valueMax = this.dataTable[this.dataTable.length - 1].Pay;
    this.valueMin = this.dataTable[0].Pay;
  }
  calcTotal() {
    this.validateForm.value.AmountPaid = this.valueMax;
  }

  //Chọn ngày thanh toán thực tế
  selectDay() {
    const selectedDate = this.validateForm.value.PaymentDate;
    this.getExcelTable(selectedDate);
  }

  tableItemRefChange() {
    let date = new Date();
    if (this.validateForm.value.Date) date = this.validateForm.value.Date;
    this.getExcelTable(this.newDate);
  }

  //tính tiền
  calcPayCount(event: number) {
    const inputValue = event;
    if (this.dataTable.length && inputValue <= this.valueMin) this.payCount = 1;
    else
      for (let i = 0; i < this.dataTable.length; i++) {
        if (inputValue <= this.dataTable[i].Pay) {
          if ((this.dataTable[i].Pay - this.dataTable[i - 1].Pay) / 2 + this.dataTable[i - 1].Pay < inputValue) {
            this.payCount = i + 1;
            break;
          } else {
            this.payCount = i;
            break;
          }
        }
      }
  }
}
