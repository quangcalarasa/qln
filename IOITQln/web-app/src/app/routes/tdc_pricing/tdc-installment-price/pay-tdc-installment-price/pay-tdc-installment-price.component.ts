import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { convertDate, convertMoney } from 'src/app/infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { PayInstallMentPriceRepository } from 'src/app/infrastructure/repositories/pay-tdc-installment-price.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { truncateSync } from 'fs';

@Component({
  selector: 'app-pay-tdc-installment-price',
  templateUrl: './pay-tdc-installment-price.component.html',
  styles: []
})
export class PayTdcInstallmentPriceComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

  moneyValue: string;
  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data: any[] = [];
  payCountPayOff: number = 1;
  dataTable: any[] = [];
  dataTableClone: any[] = [];
  newDate = convertDate(new Date().toString());

  date = new Date();
  arr: any[] = [];
  input: number[] = [];
  total: number;
  valueMax: number;
  valueMin: number;
  payCount: number;
  isPublicPay: boolean = false;
  isPayOff: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'ValueHeader', render: 'Value', className: 'text-center', width: 500 },
    { renderTitle: 'DateHeader', render: 'Date', className: 'text-center' },
    { renderTitle: 'NoteHeader', render: 'Note', className: 'text-center', width: 150 },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'delete',
          iif: i => !i.edit,
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá bản ghi này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];
  loading: boolean = false;
  @Input() record: NzSafeAny;

  constructor(
    private payInstallMentPriceRepository: PayInstallMentPriceRepository,
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
    private modalSrv: NzModalService,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.getData();
    this.validateForm = this.fb.group({
      Date: [new Date().toISOString().slice(0, 10)],
      Value: undefined,
      PayTime: undefined,
      TdcInstallmentPriceId: this.record.Id,
      PayCount: undefined,
      PublicPay: undefined,
      IsPayOff: undefined
    });
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.Value = this.validateForm.value.Value;
    data.PayCount = this.payCount;
    if (data.IsPayOff) {
      data.PayCount = this.payCountPayOff;
    }
    data.PublicPay = this.validateForm.value.PublicPay;
    const resp = await this.payInstallMentPriceRepository.addNew(data);
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
  onInputChange(event: Event): void { }
  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `TdcInstallmentPriceId=${this.record.Id}`;
    this.paging.select = 'Value,Date,PayCount,Id';
    try {
      this.loading = true;
      const resp = await this.payInstallMentPriceRepository.getByPage(this.paging);

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
    const resp = await this.payInstallMentPriceRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa thành công!`);
      this.drawerRef.close();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  changeDatePay()
  { 
    for(let i=0;i<this.dataTable.length;i++)
    {
      if (this.dataTable[i])
      for(let j=0;j<this.dataTable[i].tdcInstallmentPriceTables.length;j++)
      {
        if(this.dataTable[i].tdcInstallmentPriceTables[j].RowStatus == 3)
           {
            this.dataTable[i].tdcInstallmentPriceTables[j].PayDateGuess = this.validateForm.value.Date;
           }
      }
    }
  }
  async getWorkSheet(date: any) {
    const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(this.record.Id, date, true);
    if (resp.meta?.error_code == 200) {
      this.dataTable = resp.data.filter(
        (i: any) =>
          i.DataStatus === 2 &&
          i.tdcInstallmentPriceTables[0].RowStatus != 5 &&
          i.tdcInstallmentPriceTables[0].PayDateDefault <= this.validateForm.value.Date
      );
      this.dataTableClone = resp.data.filter(
        (i: any) =>
          i.DataStatus === 2 &&
          i.tdcInstallmentPriceTables[0].RowStatus != 5 &&
          i.tdcInstallmentPriceTables[0].PayDateDefault > this.validateForm.value.Date
      );
      this.dataTable.push(this.dataTableClone[0]);
    }
    this.dataTable[0].Pay = Math.round(this.dataTable[0].Pay);
    this.input.push(this.dataTable[0].Pay);
    for (let i = 0; i < this.dataTable.length; i++) {
      if (this.dataTable[i]) {
        if (this.dataTable[i].Pay != null)
          if (i > 0) {
            this.dataTable[i].Pay = Math.round(this.dataTable[i].Pay + this.dataTable[i - 1].Pay);
            this.input.push(this.dataTable[i].Pay);
          }
        this.dataTable[i].Paid = Math.round(this.dataTable[i].Paid);
        if (this.dataTable[i].PriceDifference != null) this.dataTable[i].PriceDifference = Math.round(this.dataTable[i].PriceDifference);
      }
    }
    this.changeDatePay();
    this.valueMax = this.dataTable[this.dataTable.length - 1].Pay;
    this.valueMin = this.dataTable[0].Pay;
  }

  calcTotal() {
    this.validateForm.value.Value = this.valueMax;
    return convertMoney(this.valueMax);
  }

  tableItemRefChange() {
    let date = new Date();
    if (this.validateForm.value.Date) date = this.validateForm.value.Date;
    this.getWorkSheet(date);
  }

  selectDay() {
    const selectedDate = this.validateForm.value.Date;
    if (selectedDate != undefined && selectedDate != '') this.getWorkSheet(selectedDate);
    this.changeDatePay();
  }

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

  onCheckboxChange() {
    this.isPublicPay = !this.isPublicPay;
    console.log(this.isPublicPay); // In giá trị mới của isPublicPay
    this.validateForm.value.PublicPay = this.isPublicPay;
    console.log(this.validateForm.value.PublicPay);
    // Các xử lý khác tùy theo logic của bạn
  }

  async onChange() {
    this.isPayOff = !this.isPayOff;
    this.payCountPayOff = this.dataTable.length;
    let date = new Date();
    if (this.validateForm.value.Date) date = this.validateForm.value.Date;
    if (this.isPayOff) {
      const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(this.record.Id, date, true, true, this.dataTable.length);
      if (resp.meta?.error_code == 200) {
        this.dataTable = []
        this.dataTable.push(resp.data[resp.data.length - 1]);
      }
    } else {
      const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(this.record.Id, date, true);
      if (resp.meta?.error_code == 200) {
        this.dataTable = resp.data.filter(
          (i: any) =>
            i.DataStatus === 2 &&
            i.tdcInstallmentPriceTables[0].RowStatus != 5 &&
            i.tdcInstallmentPriceTables[0].PayDateDefault <= this.validateForm.value.Date
        );
        this.dataTableClone = resp.data.filter(
          (i: any) =>
            i.DataStatus === 2 &&
            i.tdcInstallmentPriceTables[0].RowStatus != 5 &&
            i.tdcInstallmentPriceTables[0].PayDateDefault > this.validateForm.value.Date
        );
        this.dataTable.push(this.dataTableClone[0]);
      }
    }
    for (let i = 0; i < this.dataTable.length; i++) {
      if (this.dataTable[i]) {
        let date: any;
        for (let j = 0; j < this.dataTable[i].tdcInstallmentPriceTables.length; j++) {
          if (this.dataTable[i].tdcInstallmentPriceTables[j].RowStatus == 5) {
            this.dataTable[i].tdcInstallmentPriceTables[j].PayDateDefault = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].PayDateBefore = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].PayDateGuess = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].PayDateReal = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].MonthInterestRate = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].DailyInterestRate = null;
            this.dataTable[i].tdcInstallmentPriceTables[j].TotalPay = null;
            this.dataTable[i].Pay = null;
            this.dataTable[i].PriceDifference = null;
          }
        }
        if (this.dataTable[i].Pay != null)
          if (i > 0) {
            this.dataTable[i].Pay = Math.round(this.dataTable[i].Pay + this.dataTable[i - 1].Pay);
            this.input.push(this.dataTable[i].Pay);
          }
        this.dataTable[i].Paid = Math.round(this.dataTable[i].Paid);
        if (this.dataTable[i].PriceDifference != null) this.dataTable[i].PriceDifference = Math.round(this.dataTable[i].PriceDifference);
      }
    }
    this.valueMax = this.dataTable[this.dataTable.length - 1].Pay;
    this.valueMin = this.dataTable[0].Pay;
  }
}
