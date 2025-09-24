import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { ConversionRepository } from 'src/app/infrastructure/repositories/conversion.repository';
import { TypeCoefficient, TypeQD } from 'src/app/shared/utils/consts';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-add-or-update-conversion',
  templateUrl: './add-or-update-conversion.component.html',
  styles: []
})
export class AddOrUpdateConversionComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeCoefficient = TypeCoefficient;
  TypeQD = TypeQD;

  @Input() record: NzSafeAny;
  @Input() decree_typies: NzSafeAny;
  role = this.commonService.CheckAccessKeyRole(AccessKey.CONVERSION);
  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private conversionRepository: ConversionRepository,private commonService: CommonService,) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      TypeQD: [this.record ? this.record.TypeQD.toString() : undefined, [Validators.required]],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      CoefficientName: [this.record ? this.record.CoefficientName.toString() : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    const resp = data.Id ? await this.conversionRepository.update(data) : await this.conversionRepository.addNew(data);
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
}
