import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeCoefficient } from 'src/app/shared/utils/consts';
import { DiscountCoefficientRepository } from 'src/app/infrastructure/repositories/discount-coefficient.repositories';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-discount',
  templateUrl: './add-or-update-discount.component.html',
  styles: []
})
export class AddOrUpdateDiscountComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @Input() unit_price_data: NzSafeAny;
  TypeCoefficient = TypeCoefficient;
  role = this.commonService.CheckAccessKeyRole(AccessKey.DISCOUNT_COFFICIENT);
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private discountCoefficientRepository: DiscountCoefficientRepository,
    private cdr: ChangeDetectorRef,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      UnitPriceId: [this.record ? this.record.UnitPriceId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
      DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, [Validators.required]]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.discountCoefficientRepository.update(data) : await this.discountCoefficientRepository.addNew(data);
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
