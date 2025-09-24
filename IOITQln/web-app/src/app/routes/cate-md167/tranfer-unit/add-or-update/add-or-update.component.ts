import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167TranferUnitRepository } from 'src/app/infrastructure/repositories/md167-tranfer-unit.repository';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateTranferUnitComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167TranferUnitRepository: Md167TranferUnitRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
    });
  }


  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    const resp = data.Id ? await this.md167TranferUnitRepository.update(data) : await this.md167TranferUnitRepository.addNew(data);
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
