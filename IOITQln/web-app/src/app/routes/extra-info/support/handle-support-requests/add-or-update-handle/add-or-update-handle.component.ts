import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TypePersonSupportName, TypeSupportReq } from 'src/app/shared/utils/consts';
import { ExtraSupportHandleRepository } from 'src/app/infrastructure/repositories/extra-support-handle.repository';
import { ExtraSupportRequestRepository } from 'src/app/infrastructure/repositories/extra-support-requests.repository';

@Component({
  selector: 'app-add-or-update-handle',
  templateUrl: './add-or-update-handle.component.html',
  styles: [
  ]
})
export class AddOrUpdateHandleComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypePersonSupport = TypePersonSupportName;

  @Input() record: NzSafeAny;
  @Input() typesup_data: NzSafeAny;
  @Input() typepersup_data: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private extraSupportHandleRepository: ExtraSupportHandleRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      House: [this.record ? this.record.House : undefined, [Validators.required]],
      Apartment: [this.record ? this.record.Apartment : undefined, [Validators.required]],
      RequirePerson: [this.record ? this.record.RequirePerson : undefined, [Validators.required]],
      TypeSupport: [this.record ? this.record.TypeSupport : undefined, [Validators.required]],
      TypePersonSupportName: [this.record ? this.record.TypePersonSupportName.toString() : undefined, [Validators.required]],
      Title: [this.record ? this.record.Title : undefined, [Validators.required]],
      ToDate: [this.record ? convertDate(this.record.ToDate) : undefined, [Validators.required]],
      Content: [this.record ? this.record.Content : undefined, [Validators.required]]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    console.log(data);
    const resp = data.Id ? await this.extraSupportHandleRepository.update(data) : await this.extraSupportHandleRepository.addNew(data);
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
