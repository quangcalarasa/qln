import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TypeUsageStatus } from 'src/app/shared/utils/consts';
import { ExtraHistoryAndStatusRepository } from 'src/app/infrastructure/repositories/extrahistoryandstatus.repository';

@Component({
  selector: 'app-add-usage-history',
  templateUrl: './add-usage-history.component.html',
  styles: [
  ]
})
export class AddUsageHistoryComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeUsageStatus = TypeUsageStatus;

  @Input() record: NzSafeAny;
  @Input() typeusagestatus_data: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private extraHistoryAndStatusRepository: ExtraHistoryAndStatusRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Code: [this.record ? this.record.Code : undefined],
      House: [this.record ? this.record.House : undefined, [Validators.required]],
      Apartment: [this.record ? this.record.Apartment : undefined, [Validators.required]],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
      Total: [this.record ? this.record.Total : undefined, [Validators.required]],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      ToDate: [this.record ? convertDate(this.record.ToDate) : undefined, [Validators.required]],
      Phone: [this.record ? this.record.Phone : undefined, [Validators.required]],
      TypeUsageStatusName: [this.record ? this.record.TypeUsageStatusName.toString() : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.extraHistoryAndStatusRepository.update(data) : await this.extraHistoryAndStatusRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

}

