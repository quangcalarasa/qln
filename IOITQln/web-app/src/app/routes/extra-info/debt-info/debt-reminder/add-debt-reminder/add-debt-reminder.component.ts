import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { ExtraDebtReminderRepository } from 'src/app/infrastructure/repositories/extra-debt-reminder.repository';

@Component({
  selector: 'app-add-debt-reminder',
  templateUrl: './add-debt-reminder.component.html',
  styles: [
  ]
})
export class AddDebtReminderComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() event: NzSafeAny;
  data_district_filter: NzSafeAny = [];

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private extraDebtReminderRepository: ExtraDebtReminderRepository,
   ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.event ? this.event.Id : undefined],
      DebtRemindNumber: [this.event ? this.event.DebtRemindNumber : undefined, [Validators.required]],
      Date: [this.event ? this.event.Date : undefined, [Validators.required]],
      Code: [this.event ? this.event.Code : undefined, [Validators.required]],
      House: [this.event ? this.event.House : undefined, [Validators.required]],
      Apartment: [this.event ? this.event.Apartment : undefined, [Validators.required]],
      Address: [this.event ? this.event.Address : undefined, [Validators.required]],
      Owner: [this.event ? this.event.Owner : undefined, [Validators.required]],
      SDT: [this.event ? this.event.SDT : undefined, [Validators.required]],
      Content: [this.event ? this.event.Content : undefined, [Validators.required]],
      Times: [this.event ? this.event.Times : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.extraDebtReminderRepository.update(data) : await this.extraDebtReminderRepository.addNew(data);
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
}


