import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
  selector: 'app-add-or-update-ward',
  templateUrl: './add-or-update-ward.component.html'
})
export class AddOrUpdateWardComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() data_province: NzSafeAny;
  @Input() data_district: NzSafeAny;
  @Input() isUpdateName: NzSafeAny;

  data_district_filter: NzSafeAny = [];

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private wardRepository: WardRepository) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      ProvinceId: [this.record ? this.record.ProvinceId : undefined, [Validators.required]],
      DistrictId: [this.record ? this.record.DistrictId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined]
    });

    if (this.record) {
      this.data_district_filter = this.data_district.filter((x: any) => x.ProvinceId == this.validateForm.value.ProvinceId);
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? (this.isUpdateName == true ? await this.wardRepository.updateName(data) : await this.wardRepository.update(data)): await this.wardRepository.addNew(data);
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

  districtByProvinceId() {
    this.data_district_filter = [];
    this.validateForm.get('DistrictId')?.setValue(undefined);

    if (this.validateForm.value.ProvinceId) {
      this.data_district_filter = this.data_district.filter((x: any) => x.ProvinceId == this.validateForm.value.ProvinceId);
    }
  }
}
