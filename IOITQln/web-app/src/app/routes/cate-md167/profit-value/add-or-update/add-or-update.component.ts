import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167ProfitValueRepository } from 'src/app/infrastructure/repositories/md167-profit-value.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateMd167ProfitValueComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @Input() unit_price_data: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private md167ProfitValueRepository: Md167ProfitValueRepository,
    private cdr: ChangeDetectorRef
  ) { }

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

    const resp = data.Id ? await this.md167ProfitValueRepository.update(data) : await this.md167ProfitValueRepository.addNew(data);
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
