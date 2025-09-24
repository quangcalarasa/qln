import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TimeCoefficientRepository } from 'src/app/infrastructure/repositories/time-coeficient.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-add-or-update-time-coefficient',
  templateUrl: './add-or-update-time-coefficient.component.html',
  styles: []
})
export class AddOrUpdateTimeCoefficientComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @Input() unit_price_data: NzSafeAny;
  role = this.commonService.CheckAccessKeyRole(AccessKey.COEFFICIENT);
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private timeCoefficientRepository: TimeCoefficientRepository,
    private cdr: ChangeDetectorRef,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, [Validators.required]],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      UnitPriceName: [this.record ? this.record.UnitPriceName : undefined],
      UnitPriceId: [this.record ? this.record.UnitPriceId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.timeCoefficientRepository.update(data) : await this.timeCoefficientRepository.addNew(data);
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
