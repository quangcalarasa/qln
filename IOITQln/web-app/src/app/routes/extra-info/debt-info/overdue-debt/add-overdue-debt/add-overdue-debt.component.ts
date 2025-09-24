import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

import { ExtraDbetManageRepository } from 'src/app/infrastructure/repositories/ExtraDbetManage.repository';
import { convertDate } from '../../../../../infrastructure/utils/common';

@Component({
  selector: 'app-add-overdue-debt',
  templateUrl: './add-overdue-debt.component.html',
  styles: [
  ]
})
export class AddOverdueDebtComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() data_type: NzSafeAny;

  data_district_filter: NzSafeAny = [];

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private wardRepository: WardRepository,private ExtraDbetManageRepository : ExtraDbetManageRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      House: [this.record ? this.record.House : undefined, [Validators.required]],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
      FullName: [this.record ? this.record.FullName : undefined, [Validators.required]],
      Phone: [this.record ? this.record.Phone : undefined, [Validators.required]],
      Period: [this.record ? this.record.Period : undefined, [Validators.required]],
      Price: [this.record ? this.record.Price : undefined, [Validators.required]],
      DatePay : [this.record ? convertDate(this.record.DatePay) : undefined, [],],
      DaysOverdue: [this.record ? this.record.DaysOverdue : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ExtraDbetManageRepository.update(data) : await this.ExtraDbetManageRepository.addNew(data);
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

  districtByProvinceId() {
  }
}

