import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { DefaultCoefficientRepository } from 'src/app/infrastructure/repositories/default-coefficient.repositories';
import { TypeCoefficient } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-add-or-update-default-coefficient',
  templateUrl: './add-or-update-default-coefficient.component.html',
  styles: []
})
export class AddOrUpdateDefaultCoefficientComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @Input() unit_price_data: NzSafeAny;
  TypeCoefficient = TypeCoefficient;
  role = this.commonService.CheckAccessKeyRole(AccessKey.DEFAULTCOEFFICIENT);
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private defaultCoefficientRepository: DefaultCoefficientRepository,
    private cdr: ChangeDetectorRef,
    private commonService: CommonService,
    ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      CoefficientName: [this.record ? this.record.CoefficientName.toString() : undefined, [Validators.required]],
      Content: [this.record ? this.record.Content : undefined, [Validators.required]],
      UnitPriceId: [this.record ? this.record.UnitPriceId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
      Status_use : [this.record? this.record.Status_use : undefined]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    console.log(data);
    const resp = data.Id ? await this.defaultCoefficientRepository.update(data) : await this.defaultCoefficientRepository.addNew(data);
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

  onChange(){
    this.validateForm.get('Status_use')?.setValue(true);
  }
}
