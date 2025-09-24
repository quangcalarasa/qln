import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { PlatformTdcRepository } from 'src/app/infrastructure/repositories/platform-tdc.repository';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';

@Component({
  selector: 'app-add-or-update-platformtdc',
  templateUrl: './add-or-update-platformtdc.component.html',
  styles: [
  ]
})
export class AddOrUpdatePlatformTdcComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  myForm = new FormGroup({
    Corner: new FormControl(false)
  });

  @Input() record: NzSafeAny;
  @Input() land_data: NzSafeAny;

  lengtharea = 0; 
  widtharea = 0;
  arealand = 0;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private platformTdcRepository: PlatformTdcRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      Corner: [this.record ? this.record.Corner : undefined, []],
      LandArea: [this.record ? this.record.LandArea : undefined, [Validators.required]],
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
      Platcount: [this.record ? this.record.Platcount : undefined, [Validators.required]],
      LengthArea: [this.record ? this.record.LengthArea : undefined, [Validators.required]],
      WidthArea: [this.record ? this.record.WidthArea : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
    });
    this.lengtharea = this.record ? this.record.LengthArea : 0;
    this.widtharea = this.record ? this.record.WidthArea : 0;

    this.validateForm.get('LengthArea')?.valueChanges.subscribe((value) => {
      this.lengtharea = value;
      this.calculateLandArea();
    });

    this.validateForm.get('WidthArea')?.valueChanges.subscribe((value) => {
      this.widtharea = value;
      this.calculateLandArea();
    });
  
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const isConner = this.validateForm.get('Corner')?.value;

    data.isConner = isConner;

    const resp = data.Id ? await this.platformTdcRepository.update(data) : await this.platformTdcRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }

  calculateLandArea(): void {
    this.validateForm.patchValue({
      LandArea: this.lengtharea * this.widtharea
    });
  }

  close(): void {
    this.drawerRef.close();
  }

}
