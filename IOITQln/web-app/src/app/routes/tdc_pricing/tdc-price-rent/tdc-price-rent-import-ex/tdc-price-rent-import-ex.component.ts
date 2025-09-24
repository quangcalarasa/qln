import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import * as XLSX from 'xlsx';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';
import { convertDate } from '../../../../infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';

@Component({
  selector: 'app-tdc-price-rent-import-ex',
  templateUrl: './tdc-price-rent-import-ex.component.html'
})
export class TdcPriceRentImportExComponent implements OnInit {
  @Input() record: NzSafeAny;
  dataImport: any[] = [];

  columns: string[] = [
    'PaymentTimes',
    'PaymentDatePrescribed',
    'PaymentDatePrescribed1',
    'ExpectedPaymentDate',
    'DailyInterest',
    'DailyInterestRate',
    'UnitPay',
    'PriceEarnings',
    'PricePaymentPeriod',
    'Pay',
    'Paid',
    'PriceDifference',
    'Note'
  ];
  data: any[] = [];
  dataTable: {
    PayTime: any;
    DataRow: any;
    Pay: any;
    Paid: any;
    PriceDifference: any;
    Check: any;
    tdcPriceRentExcelDatas: any[];
  }[] = [];
  loading: boolean = false;
  constructor(private drawerRef: NzDrawerRef<string>, private tdcPriceRentRepository: TdcPriceRentRepository) {}

  ngOnInit(): void {}

  convertExcelNumberToDate(excelNumber: number): any {
    const timestamp = (excelNumber - 2) * (1000 * 60 * 60 * 24) + new Date('1900-01-01').getTime();
    const dateObj = new Date(timestamp);
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    const dateStr = `${month}/${day}/${year}`;
    const date = convertDate(dateStr);
    return date;
  }

  onFileChange(event: any) {
    const target: DataTransfer = event.target;
    if (target.files.length !== 1) throw new Error('Cannot use multiple files');

    const reader: FileReader = new FileReader();
    reader.onload = (e: any) => {
      const bstr: string = e.target.result;
      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];
      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      const jsonData: any[] = XLSX.utils.sheet_to_json(ws, { header: 1 });

      this.data = jsonData.slice(0).map((row: any[]) => {
        const obj: any = {};

        for (let i = 0; i < this.columns.length; i++) {
          const column = this.columns[i];
          let value;
          if (column === 'ExpectedPaymentDate' && typeof row[i] === 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          } else if (column === 'ExpectedPaymentDate' && typeof row[i] === undefined) {
            value = null;
          } else if (column === 'PaymentDatePrescribed' && typeof row[i] === 'string') {
            value = undefined;
          } else if (column === 'PaymentDatePrescribed' && typeof row[i] === 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          } else if (column === 'DailyInterestRate' && typeof row[i] == 'number') {
            value = row[i] * 100;
          } else if (column === 'PaymentTimes' && typeof row[i] === 'string') {
            value = row[i].toString();
          } else if (column === 'PriceDifference' && typeof row[i] === 'string') {
            value = 0;
          } else value = row[i];
          obj[column] = value;
        }
        return obj;
      });
      let y = 0;
      for (let i = 0; i < this.data.length; i++) {
        this.data[i].index = i;
        if (i > 0 && typeof this.data[i].PaymentTimes === 'string') {
          y = y + 1;
        }
      }
      this.data[0].CountYear = y;

      let j = 2;
      for (let i = 3; i < this.data.length; i++) {
        this.data[i].RowSpans = 1;
        if (this.data[i].ExpectedPaymentDate == this.data[j].ExpectedPaymentDate || this.data[i].ExpectedPaymentDate == undefined) {
          this.data[i].RowCheck = false;
          this.data[j].RowSpans = this.data[j].RowSpans + 1;
        } else {
          this.data[i].RowCheck = true;
          j = i;
        }
      }
      let k = 2;
      for (let i = 3; i < this.data.length; i++) {
        this.dataTable[0] = {
          PayTime: this.data[0].PaymentTimes,
          DataRow: 1,
          Pay: this.data[0].Pay,
          Paid: this.data[0].Paid,
          PriceDifference: null,
          Check: true,
          tdcPriceRentExcelDatas: []
        };
        this.dataTable[0].tdcPriceRentExcelDatas.push(this.data[0]);

        this.dataTable[1] = {
          PayTime: this.data[1].PaymentTimes,
          DataRow: 1,
          Pay: this.data[1].Pay,
          Paid: this.data[1].Paid,
          PriceDifference: null,
          Check: true,
          tdcPriceRentExcelDatas: []
        };
        this.dataTable[1].tdcPriceRentExcelDatas.push(this.data[1]);

        this.dataTable[2] = {
          PayTime: this.data[2].PaymentTimes,
          DataRow: 1,
          Pay: this.data[2].Pay,
          Paid: this.data[2].Paid,
          PriceDifference: this.data[2].PriceDifference,
          Check: false,
          tdcPriceRentExcelDatas: []
        };
        this.dataTable[2].tdcPriceRentExcelDatas.push(this.data[2]);

        const pay = this.data[i].Pay;
        if (pay !== undefined) {
          k++;
          this.dataTable[k] = {
            PayTime: this.data[i].PaymentTimes,
            DataRow: 1,
            Pay: this.data[i].Pay,
            Paid: this.data[i].Paid,
            PriceDifference: this.data[i].PriceDifference,
            Check: false,
            tdcPriceRentExcelDatas: []
          };
          this.dataTable[k].tdcPriceRentExcelDatas.push(this.data[i]);
        } else {
          this.dataTable[k].tdcPriceRentExcelDatas.push(this.data[i]);
        }
      }
    };
    reader.readAsBinaryString(target.files[0]);
  }

  async onSubmit() {
    let data = [...this.dataTable];
    const resp = await this.tdcPriceRentRepository.Import(data, this.record.Id);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }
  close() {
    this.drawerRef.close();
  }
}
