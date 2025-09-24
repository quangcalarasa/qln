import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ExtraDebtNotificationRepository } from 'src/app/infrastructure/repositories/extradebtnotification.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeNotificationForm } from 'src/app/shared/utils/consts';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-add-debt-notification',
  templateUrl: './add-debt-notification.component.html',
  styles: [
  ]
})
export class AddDebtNotificationComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeNotificationForm = TypeNotificationForm
  @Input() record: NzSafeAny;
  @Input() notificationform_data: NzSafeAny;

  data_district_filter: NzSafeAny = [];

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private extraDebtNotificationRepository: ExtraDebtNotificationRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      DebtType: [this.record ? this.record.DebtType : undefined],
      TypeNotificationForm: [this.record ? this.record.TypeNotificationForm.toString() : undefined, [Validators.required]],
      Title: [this.record ? this.record.Title : undefined, [Validators.required]],
      Content: [this.record ? this.record.Content : undefined, [Validators.required]],
      GroupNotification: [this.record ? this.record.GroupNotification : undefined, [Validators.required]],
      ListNotification: [this.record ? this.record.ListNotification : undefined, [Validators.required]],
      ToDate: [this.record ? convertDate(this.record.ToDate) : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.extraDebtNotificationRepository.update(data) : await this.extraDebtNotificationRepository.addNew(data);
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

