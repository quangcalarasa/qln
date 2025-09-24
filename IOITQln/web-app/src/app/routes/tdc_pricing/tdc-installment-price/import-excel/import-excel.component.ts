import { Component, Input, OnInit } from '@angular/core';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import * as XLSX from 'xlsx';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { convertExcelNumberToDate,CompareString } from 'src/app/infrastructure/utils/common';



@Component({
  selector: 'app-import-excel',
  templateUrl: './import-excel.component.html',
  styles: [
  ]
})
export class ImportExcelComponent implements OnInit {
  @Input() record: NzSafeAny;
  columns: string[] = [
    'PaymentTimes',
    'PayDateDefault',
    'PayDateBefore',
    'PayDateGuess',
    'PayDateReal',
    'MonthInterest',
    'DailyInterest',
    'MonthInterestRate',
    'DailyInterestRate',
    'TotalPay',
    'PayAnnual',
    'TotalInterest',
    'TotalPayAnnual',
    'Pay',
    'Paid',
    'PriceDifference',
    'Note',
  ];
  data: any[] = [];
  dataTable: {
    PayTime: any;
    DataRow: any,
    Pay: any,
    Paid: any,
    PriceDifference: any,
    installmentPriceTableTdcs: any[]
  }[] = [];
  selectedFile: File | undefined;
  loading: boolean = false;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
  ) { }

  ngOnInit(): void {

  }
  onFileChange(event: any) {
    this.selectedFile = event.target.files[0];
    const target: DataTransfer = event.target;
    if (target.files.length !== 1) throw new Error('Cannot use multiple files');

    const reader: FileReader = new FileReader();
    reader.onload = (e: any) => {
      const bstr: string = e.target.result;
      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];
      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      const jsonData: any[] = XLSX.utils.sheet_to_json(ws, { header: 1 });

      this.data = jsonData.slice(3).map((row: any[]) => {
        const obj: any = {};
        for (let i = 0; i < this.columns.length; i++) {
          const column = this.columns[i];
          let value;
          if (column === 'PayDateDefault' && row[i] !== undefined && typeof row[i] !== 'string') {
            value = convertExcelNumberToDate(row[i]);
          }
          else if (column === 'PayDateBefore' && row[i] !== undefined) {
            value = convertExcelNumberToDate(row[i]);
          }
          else if (column === 'PayDateGuess' && row[i] !== undefined) {
            value = convertExcelNumberToDate(row[i]);
          }
          else if (column === 'PayDateReal' && row[i] !== undefined) {
            value = convertExcelNumberToDate(row[i]);
          }
          else if (column === 'MonthInterestRate' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(",", "."));
          }
          else if (column === 'DailyInterestRate' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(",", "."));
          }
          else if (column === 'Paid' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else if (column === 'Pay' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else if (column === 'PayAnnual' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else if (column === 'PriceDifference' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/[()]/g, "").replace(/\./g, ""));
          }
          else if (column === 'TotalInterest' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else if (column === 'TotalPay' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else if (column === 'TotalPayAnnual' && typeof row[i] === 'string') {
            value = parseFloat(row[i].replace(/\./g, ""));
          }
          else
            value = row[i];

          if (typeof value === 'number' && isNaN(value))
            value = null
          obj[column] = value;
        }
        return obj;
      });
      let j = -1;
      for (let i = 0; i < this.data.length; i++) {
        this.data[i].RowStatus = 1; 
        if (this.data[i].DailyInterestRate)
          this.data[i].DailyInterestRate = Math.round(this.data[i].DailyInterestRate * Math.pow(10, 5)) / Math.pow(10, 5);
        if (this.data[i].MonthInterestRate)
          this.data[i].MonthInterestRate = Math.round(this.data[i].MonthInterestRate * Math.pow(10, 5)) / Math.pow(10, 5);
        if (this.data[i].Paid)
          this.data[i].Paid = Math.round(this.data[i].Paid)
        if (this.data[i].Pay)
          this.data[i].Pay = Math.round(this.data[i].Pay)
        if (this.data[i].TotalPay)
          this.data[i].TotalPay = Math.round(this.data[i].TotalPay)
        if (this.data[i].TotalPayAnnual)
          this.data[i].TotalPayAnnual = Math.round(this.data[i].TotalPayAnnual)
        if (this.data[i].PriceDifference)
          this.data[i].PriceDifference = Math.round(this.data[i].PriceDifference)
        if (this.data[i].TotalInterest)
          this.data[i].TotalInterest = Math.round(this.data[i].TotalInterest)
        if (this.data[i].PayAnnual)
          this.data[i].PayAnnual = Math.round(this.data[i].PayAnnual)
        if(this.data[i].PayDateDefault!=undefined)
          if (CompareString(this.data[i].PayDateDefault,'Trễ hạn')) {
            this.data[i].PayDateDefault = undefined;
            this.data[i].RowStatus = 3;
          }
        if(this.data[i].PayDateDefault!=undefined)
          if (CompareString(this.data[i].PayDateDefault,'Nợ cũ')) {
            this.data[i].PayDateDefault = undefined;
            this.data[i].RowStatus = 2;
          }
        if (typeof this.data[i].PaymentTimes === 'string') this.data[i].check = true;

        const paymentTimes = this.data[i].PaymentTimes;
        const pay = this.data[i].Pay;


        if (pay !== undefined || typeof paymentTimes === 'string'|| CompareString(this.data[i].Note==undefined?"":this.data[i].Note,'Thanh lý HĐ cũ')) {
          j++;
          this.dataTable[j] = {
            PayTime: this.data[i].PaymentTimes,
            DataRow: 1,
            Pay: this.data[i].Pay,
            Paid: this.data[i].Paid,
            PriceDifference: this.data[i].PriceDifference,
            installmentPriceTableTdcs: []
          };
          this.dataTable[j].installmentPriceTableTdcs.push(this.data[i]);
        }
        else {
          this.dataTable[j].installmentPriceTableTdcs.push(this.data[i]);
        }
      }

    };
    reader.readAsBinaryString(target.files[0]);
  }
  btn() {
    console.log(this.data);

  }
  async submitForm() {
    let k = 0;
    let data = [...this.dataTable];
    for (let i = 0; i < data.length; i++) {
      for (let j = 0; j < data[i].installmentPriceTableTdcs.length; j++) {
        if (typeof data[i].installmentPriceTableTdcs[j].PaymentTimes === 'string') {
          if (CompareString(data[i].installmentPriceTableTdcs[j].PaymentTimes,'Tổng')) {
            data[i].installmentPriceTableTdcs[j].PaymentTimes = undefined;
            data[i].installmentPriceTableTdcs[j].DataRow = 2;
            data[i].installmentPriceTableTdcs[j].RowStatus = 5;
          }
          else {
            data[i].installmentPriceTableTdcs[j].PaymentTimes = undefined;
            data[i].installmentPriceTableTdcs[j].DataRow = 3;
          }
        }
        if (typeof data[i].installmentPriceTableTdcs[0].PaymentTimes === 'number' && data[i].Paid === undefined) {
          data[i].DataRow = 4;
          let foundItem = data.find(item => item.PayTime === data[i].PayTime - 1);
          if (foundItem) {
            foundItem.DataRow = foundItem.DataRow || {};
            foundItem.DataRow = 7;
          }
        }
        k++;
        data[i].installmentPriceTableTdcs[j].Location = k;
      }
    }
    console.log(data);
    const resp = await this.tDCInstallmentPriceRepository.Import(data, this.record.Id);
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
