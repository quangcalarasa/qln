import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167LandTaxRepository } from 'src/app/infrastructure/repositories/md167landtax.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateMd167LandTaxComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;
  @Input() statusDefault: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167LandTaxRepository: Md167LandTaxRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      TypeArea: [this.record ? this.record.TypeArea : undefined, [Validators.required]],
      Tax: [this.record ? this.record.Tax : undefined, [Validators.required]],
      DecisionId: [this.record ? this.record.DecisionId.toString() : undefined, [Validators.required]],
      IsDefault: [this.record ? this.record.IsDefault : undefined, []],
    });
    if (this.statusDefault) {
      this.validateForm.get('IsDefault')?.setValue(false);
      this.validateForm.get('IsDefault')?.disable();
    }
    if (this.record && this.record.IsDefault) {
      this.validateForm.get('IsDefault')?.setValue(true);
      this.validateForm.get('IsDefault')?.enable();
    }
  }


  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";

    const resp = data.Id ? await this.md167LandTaxRepository.update(data) : await this.md167LandTaxRepository.addNew(data);
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

