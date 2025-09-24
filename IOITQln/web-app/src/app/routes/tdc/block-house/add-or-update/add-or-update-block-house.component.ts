import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { LandComponent } from '../../land/land.component';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update-block-house.component.html',
  styles: [
  ]
})
export class AddOrUpdateBlockHouseComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() land_data: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private blockHouseRepository: BlockHouseRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      FloorTdcCount: [{ value: this.record ? this.record.FloorTdcCount : 0, disabled: true }],
      TotalApartmentTdcCount: [{ value: this.record ? this.record.TotalApartmentTdcCount : 0, disabled: true }],
      ConstructionValue: [this.record ? this.record.ConstructionValue : undefined, [Validators.required]],
      ContrustionBuild: [this.record ? this.record.ContrustionBuild : undefined, [Validators.required]],
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.blockHouseRepository.update(data) : await this.blockHouseRepository.addNew(data);
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
