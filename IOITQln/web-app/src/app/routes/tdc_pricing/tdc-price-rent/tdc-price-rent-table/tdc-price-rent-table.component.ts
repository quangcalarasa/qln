import { Component, Input, OnInit, ChangeDetectorRef, ViewChild, Type } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
@Component({
  selector: 'app-tdc-price-rent-table',
  templateUrl: './tdc-price-rent-table.component.html',
  styles: []
})
export class TdcPriceRentTableComponent implements OnInit {
  @Input() record: NzSafeAny;
  data: any[] = [];
  data_import: any[] = [];
  newDate = convertDate(new Date().toString());
  checkedItems = [];
  validateForm!: FormGroup;

  constructor(private tdcPriceRentRepository: TdcPriceRentRepository, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Name: [this.record.TdcCustomerName],
      Aparment: [this.record.TdcApartmentName],
      Land: [this.record.TdcLandName],
      BlockHouse: [this.record.TdcBlockHouseName],
      resettlement: [],
      Floor: [this.record.TdcFloorName],
      Floor1: [this.record.Floor1],
      DatePriceTC: [convertDate(this.record.DateTTC)],
      DateTDC: [convertDate(this.record.DateTDC)],
      PriceTC: [this.record.PriceTC],
      PriceMonth: [this.record.PriceMonth]
    });
    this.getExcelTable();
  }

  async getExcelTable() {
    const resp = await this.tdcPriceRentRepository.getExcelTable(this.record.Id, this.newDate);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
    if (this.data[0].tdcPriceRentExcels[0].PaymentDatePrescribed == '0001-01-01T00:00:00') {
      this.data[0].tdcPriceRentExcels[0].PaymentDatePrescribed = null;
      this.data[0].tdcPriceRentExcels[0].PaymentDatePrescribed1 = null;
      this.data[0].tdcPriceRentExcels[0].ExpectedPaymentDate = null;
      this.data[0].tdcPriceRentExcels[0].DailyInterest = null;
      this.data[0].tdcPriceRentExcels[0].DailyInterestRate = null;
      this.data[0].tdcPriceRentExcels[0].UnitPay = null;
      this.data[0].tdcPriceRentExcels[0].PriceEarnings = null;
      this.data[0].tdcPriceRentExcels[0].PricePaymentPeriod = null;
      this.data[0].Pay = null;
      this.data[0].PriceDifference = null;
      this.data[0].tdcPriceRentExcels[0].Note = null;
    }
    for (let i = 1; i < this.data.length; i++) {
      for (let j = 0; j < this.data[i].tdcPriceRentExcels.length; j++) {
        if (
          this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed == '0001-01-01T00:00:00' &&
          this.data[i].tdcPriceRentExcels[j].ExpectedPaymentDate == '0001-01-01T00:00:00'
        ) {
          this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed = null;
          this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed1 = null;
          this.data[i].tdcPriceRentExcels[j].DailyInterest = null;
          this.data[i].tdcPriceRentExcels[j].DailyInterestRate = null;
          this.data[i].tdcPriceRentExcels[j].UnitPay = null;
          this.data[i].tdcPriceRentExcels[j].PriceEarnings = null;
          this.data[i].tdcPriceRentExcels[j].PricePaymentPeriod = null;

          this.data[i].tdcPriceRentExcels[j].Note = null;
        }
        if (
          this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed == '0001-01-01T00:00:00' &&
          this.data[i].tdcPriceRentExcels[j].ExpectedPaymentDate !== '0001-01-01T00:00:00'
        ) {
          this.data[i].tdcPriceRentExcels[j].TypeRow = 4;
        }
        if (this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed1 == '0001-01-01T00:00:00') {
          this.data[i].tdcPriceRentExcels[j].PaymentDatePrescribed1 = null;
        }
        if (this.data[i].Pay == 0) {
          this.data[i].Pay = null;
        }
        if (this.data[i].Paid == 0) {
          this.data[i].Paid = null;
        }
        if (this.data[i].PriceDifference == 0) {
          this.data[i].PriceDifference = null;
        }
      }
    }
  }
}
