import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { BlockHouseComponent } from '../../block-house/block-house.component';

@Component({
  selector: 'app-add-or-update-floortdc',
  templateUrl: './add-or-update-floortdc.component.html',
  styles: [
  ]
})
export class AddOrUpdateFloorTdcComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() blockhouse_data: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private floorTdcHouseRepository: FloorTdcRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      FloorNumber: [this.record ? this.record.FloorNumber : undefined, [Validators.required]],
      ApartmentTdcCount: [{ value: this.record ? this.record.ApartmentTdcCount : 0, disabled: true }],
      ConstructionValue: [this.record ? this.record.ConstructionValue : undefined, [Validators.required]],
      ContrustionBuild: [this.record ? this.record.ContrustionBuild : undefined, [Validators.required]],
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.floorTdcHouseRepository.update(data) : await this.floorTdcHouseRepository.addNew(data);
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
