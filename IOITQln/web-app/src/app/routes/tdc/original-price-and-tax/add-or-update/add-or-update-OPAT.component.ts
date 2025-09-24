import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { OriginalPriceAndTaxRepository } from 'src/app/infrastructure/repositories/original-price-and-tax.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-add-or-update-OPAT',
  templateUrl: './add-or-update-OPAT.component.html',
  styles: [
  ]
})
export class AddOrUpdateOPATComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  public add = true;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private originalPriceAndTaxRepository: OriginalPriceAndTaxRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    if (localStorage.getItem('add') == 'false') this.add = false
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }],
      Note: [this.record ? this.record.Note : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.originalPriceAndTaxRepository.update(data) : await this.originalPriceAndTaxRepository.addNew(data);
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
