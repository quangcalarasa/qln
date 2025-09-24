import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167AreaValueRepository } from 'src/app/infrastructure/repositories/md167area-value.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-addorupdate',
  templateUrl: './addorupdate.component.html',
  styles: [
  ]
})
export class AddorupdateAreaValueComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  lstDistrict: any;
  lstDistrictReq: any;
  nzFormat = 'dd/ MM/ yyyy';
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167AreaValueRepository: Md167AreaValueRepository,
    private modalSrv: NzModalService,
    private districtRepository: DistrictRepository,
    private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.getListDistrict();
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      Decision: [this.record ? this.record.Decision : undefined, [Validators.required]],
      EffectiveTime: [this.record ? convertDate(this.record.EffectiveTime) : undefined, [Validators.required]],
      LandPurpose: [this.record ? this.record.LandPurpose : undefined, [Validators.required]],
      md167AreaValueApplies: [this.record ? this.record.md167AreaValueApplies.map((item: any) => item.DistrictId) : undefined, [Validators.required]],
    });
    console.log(this.validateForm.value);

  }
  async getListDistrict() {
    let paging = new GetByPageModel();
    paging.query = '1=1';
    paging.order_by = 'CreatedAt Desc';
    paging.page = 1;
    paging.query = 'ProvinceId=2'
    paging.page_size = 30;
    const resp = await this.districtRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lstDistrict = resp.data;
      paging.item_count = resp.metadata;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu.'
      });
    }
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";
    data.md167AreaValueApplies = data.md167AreaValueApplies.map((item: any) => ({ DistrictId: item }));
    const resp = data.Id ? await this.md167AreaValueRepository.update(data) : await this.md167AreaValueRepository.addNew(data);
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