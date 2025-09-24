import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167VATValueRepository } from 'src/app/infrastructure/repositories/md167vat-value.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-addorupdate',
  templateUrl: './addorupdate.component.html',
  styles: [
  ]
})
export class AddorupdateVatValueComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;
  nzFormat = 'dd/ MM/ yyyy';
  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167VATValueRepository: Md167VATValueRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      EffectiveDate: [this.record ? convertDate(this.record.EffectiveDate) : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined, []],
    });
  }


  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    const resp = data.Id ? await this.md167VATValueRepository.update(data) : await this.md167VATValueRepository.addNew(data);
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
