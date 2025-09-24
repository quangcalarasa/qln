import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';

@Component({
  selector: 'app-add-or-update-apartmenttdc',
  templateUrl: './add-or-update-apartmenttdc.component.html',
  styles: [
  ]
})
export class AddOrUpdateApartmentTdcComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  myForm = new FormGroup({
    Corner: new FormControl(false)
  });

  @Input() record: NzSafeAny;
  @Input() floor_tdc_data: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private apartmentTdcRepository: ApartmentTdcRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      Corner: [this.record ? this.record.Corner : undefined, []],
      ConstructionValue: [this.record ? this.record.ConstructionValue : undefined, [Validators.required]],
      ContrustionBuild: [this.record ? this.record.ContrustionBuild : undefined, [Validators.required]],
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined, [Validators.required]],
      RoomNumber: [this.record ? this.record.RoomNumber : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const isConner = this.validateForm.get('Corner')?.value;

    data.isConner = isConner;


    const resp = data.Id ? await this.apartmentTdcRepository.update(data) : await this.apartmentTdcRepository.addNew(data);
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
