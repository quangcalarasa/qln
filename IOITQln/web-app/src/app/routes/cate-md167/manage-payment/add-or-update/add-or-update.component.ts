import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167ManPaymentRepository } from 'src/app/infrastructure/repositories/md167-manage-payment.repository';
import { HouseComponent } from '../../house/house.component';
import { HomeInfoComponent } from '../home-info/home-info.component';
import GetByPageModel from 'src/app/core/models/get-by-page-model';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateManagePaymentComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @ViewChild('homeInfoComponent') homeInfoComponent!: HomeInfoComponent;
  md167Houses: any;
  add = true;
  years: number[] = Array.from({ length: 100 }, (_, index) => 2000 + index);
  TotalTaxNN = 0;
  TotalPaid = 0;

  constructor(
    private drawerRef: NzDrawerRef<string>, 
    private fb: FormBuilder,
    private md167ManPaymentRepository: Md167ManPaymentRepository, 
    private cdr: ChangeDetectorRef
    ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      Date: [this.record ? convertDate(this.record.Date) : undefined, [Validators.required]],
      Year: [this.record ? this.record.Year : undefined, [Validators.required]],
      Payment: [this.record ? this.record.Payment : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined, []],
      housePayments: [this.record ? this.record.housePayments : []],
    });
  }

  changeData(data: any) {
    this.validateForm.get('housePayments')?.setValue(data);
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.housePayments = [...this.homeInfoComponent.getValue()];
    const resp = data.Id 
      ? await this.md167ManPaymentRepository.update(data) 
      : await this.md167ManPaymentRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }

  ChangeTotalTaxNN(event: any){
    let paided = 0;
    for(let i=0;i<event.length;i++){
      paided += event[i].Paid;
    }
    this.TotalPaid = paided;

    this.validateForm.get('Payment')?.setValue(this.TotalPaid);
  }

  close(): void {
    this.drawerRef.close();
  }
}
