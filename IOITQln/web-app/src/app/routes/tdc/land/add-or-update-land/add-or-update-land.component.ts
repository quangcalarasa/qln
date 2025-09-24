import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';



@Component({
  selector: 'app-add-or-update-land',
  templateUrl: './add-or-update-land.component.html',
  styles: [
  ]
})
export class AddOrUpdateLandComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  showLo: boolean = true;
  @Input() record: NzSafeAny;
  @Input() project_tdc_data: NzSafeAny;


  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private landRepository: LandRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }],
      Note: [this.record ? this.record.Note : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      BlockHouseCount: [{ value: this.record ? this.record.BlockHouseCount : 0, disabled: true }],
      TDCProjectId: [this.record ? this.record.TDCProjectId : undefined, [Validators.required]],
      TotalArea: [this.record ? this.record.TotalArea : undefined, [Validators.required]],
      ConstructionApartment: [this.record ? this.record.ConstructionApartment : undefined, [Validators.required]],
      ConstructionLand: [this.record ? this.record.ConstructionLand : undefined, [Validators.required]],
      ConstructionValue: [this.record ? this.record.ConstructionValue : undefined, [Validators.required]],
      ContrustionBuild: [this.record ? this.record.ContrustionBuild : undefined, [Validators.required]],
      PlotType: [this.record ? (this.record.PlotType ? this.record.PlotType.toString() : undefined) : "3", []],
      // TDCProjectName:[this.record ? this.record.TDCProjectName : undefined, [Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.landRepository.update(data) : await this.landRepository.addNew(data);
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
