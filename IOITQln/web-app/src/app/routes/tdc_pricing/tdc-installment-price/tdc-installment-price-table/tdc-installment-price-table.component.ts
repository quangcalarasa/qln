import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-tdc-installment-price-table',
  templateUrl: './tdc-installment-price-table.component.html',
  styles: []
})
export class TdcInstallmentPriceTableComponent implements OnInit {
  @Input() record: NzSafeAny;

  data: any[] = [];
  dataUpdate: any[] = [];
  newDate :any;

  constructor(private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository) { 
  }
  ngOnInit(): void {
    this.getWorkSheet();
  }
  getDate()
  {
    let newDate=new Date();
    let defaultDate=new Date(this.record.FirstPayDate);
    defaultDate.setFullYear(newDate.getFullYear());
    if(defaultDate<newDate)
    defaultDate.setFullYear(defaultDate.getFullYear()+1);
    this.newDate=convertDate(defaultDate.toString())
  } 

  btn() {
    console.log(this.data);
  }
  async getWorkSheet() {
    await this.getDate();
    const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(this.record.Id, this.newDate, false);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
      // this.data = resp.data;
    }
    for (let i = 0; i < this.data.length; i++) {
      if (this.data[i]) {
        let date: any;
        for (let j = 0; j < this.data[i].tdcInstallmentPriceTables.length; j++) {
          if (this.data[i].Paid === 0) {
            this.data[i].tdcInstallmentPriceTables[j].PayDateReal = null;
          } else {
           
          }
          if(this.data[i].tdcInstallmentPriceTables[j].RowStatus == 3)
           {
            this.data[i].tdcInstallmentPriceTables[j].PayDateGuess = this.newDate;
           }
          if (this.data[i].tdcInstallmentPriceTables[j].RowStatus == 5) {
            this.data[i].tdcInstallmentPriceTables[j].PayDateDefault = null;
            this.data[i].tdcInstallmentPriceTables[j].PayDateBefore = null;
            this.data[i].tdcInstallmentPriceTables[j].PayDateGuess = null;
            this.data[i].tdcInstallmentPriceTables[j].PayDateReal = null;
            this.data[i].tdcInstallmentPriceTables[j].MonthInterestRate = null;
            this.data[i].tdcInstallmentPriceTables[j].DailyInterestRate = null;
            this.data[i].tdcInstallmentPriceTables[j].TotalPay = null;
            this.data[i].Pay = null;
            // this.data[i].PriceDifference = null;
          }
        }
        if (this.data[i].Pay != null) this.data[i].Pay = Math.round(this.data[i].Pay);
        this.data[i].Paid = Math.round(this.data[i].Paid);
        if (this.data[i].PriceDifference != null) this.data[i].PriceDifference = Math.round(this.data[i].PriceDifference);
        if (this.data[i].Paid === 0) {
          this.data[i].PriceDifference = 0;
        }
      }
    }
  }
}
