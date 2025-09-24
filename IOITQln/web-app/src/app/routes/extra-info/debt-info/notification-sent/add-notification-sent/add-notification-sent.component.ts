import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from '../../../../../infrastructure/utils/common';
import { ExtraViewNotiSendRepository } from 'src/app/infrastructure/repositories/ExtraViewNoti.repository';

@Component({
  selector: 'app-add-notification-sent',
  templateUrl: './add-notification-sent.component.html',
  styles: [
  ]
})
export class AddNotificationSentComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,private ExtraViewNotiSendRepository : ExtraViewNotiSendRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      NotificationTitle: [this.record ? this.record.NotificationTitle : undefined, [Validators.required]],
      ContentNotification: [this.record ? this.record.ContentNotification : undefined, [Validators.required]],
      TypeNotification: [this.record ? this.record.TypeNotification : undefined, [Validators.required]],
      SendNotiBy: [this.record ?  (this.record.SendNotiBy ? this.record.SendNotiBy.toString() : undefined): undefined, [Validators.required]],
      DateSend: [this.record ? convertDate(this.record.DateSend) : undefined, [Validators.required]],
    });

   
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ExtraViewNotiSendRepository.update(data) : await this.ExtraViewNotiSendRepository.addNew(data);
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

