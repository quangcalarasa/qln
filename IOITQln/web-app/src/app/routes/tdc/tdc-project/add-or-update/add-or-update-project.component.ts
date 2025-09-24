import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { TdcProjectIngrepriceComponent } from '../tdc-project-ingreprice/tdc-project-ingreprice.component';
import { TdcProjectPriceAndTaxComponent } from '../tdc-project-price-and-tax/tdc-project-price-and-tax.component';
import GetByPageModel from 'src/app/core/models/get-by-page-model';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update-project.component.html',
  styles: [
  ]
})
export class AddOrUpdateProjectComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  pdw_data = [];
  add = true;
  lane_data: any[] = [];
  @ViewChild('tdcProjectIngrepriceComponent') tdcProjectIngrepriceComponent!: TdcProjectIngrepriceComponent;
  @ViewChild('tdcProjectPriceAndTaxComponent') tdcProjectPriceAndTaxComponent!: TdcProjectPriceAndTaxComponent;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private tDCProjectRepository: TDCProjectRepository, private cdr: ChangeDetectorRef,
    private laneRepository: LaneRepository, private provinceRepository: ProvinceRepository) { }

  ngOnInit(): void {
    if (localStorage.getItem('add') == 'false') this.add = false
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : "", disabled: true }, [Validators.required]],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      LandCount: [{ value: this.record ? this.record.LandCount : 0, disabled: true }],
      FullAddress: [this.record ? this.record.FullAddress : undefined],
      HouseNumber: [this.record ? this.record.HouseNumber : undefined, [Validators.required]],
      Lane: [this.record ? this.record.Lane : undefined],
      Ward: [this.record ? this.record.Ward : undefined],
      District: [this.record ? this.record.District : undefined],
      Province: [this.record ? this.record.Province : undefined],
      Pdw: [this.record ? [this.record.Province, this.record.District, this.record.Ward] : undefined],
      BuildingName: [this.record ? this.record.BuildingName : undefined, [Validators.required]],
      TotalAreas: [this.record ? this.record.TotalAreas : undefined, [Validators.required]],
      TotalApartment: [this.record ? this.record.TotalApartment : undefined, [Validators.required]],
      TotalPlatform: [this.record ? this.record.TotalPlatform : undefined, [Validators.required]],
      TotalFloorAreas: [this.record ? this.record.TotalFloorAreas : undefined, [Validators.required]],
      TotalUseAreas: [this.record ? this.record.TotalUseAreas : undefined, [Validators.required]],
      TotalBuildAreas: [this.record ? this.record.TotalBuildAreas : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
      LateRate: [this.record ? this.record.LateRate : 0, [Validators.required]],
      DebtRate: [this.record ? this.record.DebtRate : 0, [Validators.required]],
      tDCProjectPriceAndTaxes: [this.record ? this.record.tDCProjectPriceAndTaxes : []],
      tDCProjectIngrePrices: [this.record ? this.record.tDCProjectIngrePrices : []],
    });
    this.getCascaderData();
    if (this.record) this.getLaneData(this.record.Ward, true);

  }

  changeData(data: any) {
    this.validateForm.get('tDCProjectIngrePrices')?.setValue(data);
  }
  DeletedItemIngrePrice(data: any) {
    this.tdcProjectPriceAndTaxComponent.receiveDeletedData(data)
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.tDCProjectIngrePrices = [...this.tdcProjectIngrepriceComponent.getValue()];
    data.tDCProjectPriceAndTaxes = [...this.tdcProjectPriceAndTaxComponent.getValue()];
    const resp = data.Id ? await this.tDCProjectRepository.update(data) : await this.tDCProjectRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }
  async getCascaderData() {
    try {
      this.loading = true;
      const resp = await this.provinceRepository.getCascaderData(1);

      if (resp.meta?.error_code == 200) {
        this.pdw_data = resp.data;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  async getLaneData(wardId?: number, init: boolean = true) {
    if (!init) this.validateForm.get('Lane')?.setValue(undefined);
    this.lane_data = [];
    if (!wardId) return;

    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Ward=${wardId}`;

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_data = resp.data;

      if (resp.metadata == 1 && !init) {
        this.validateForm.get('Lane')?.setValue(this.lane_data[0].Id);
      }
    }
  }

  changePdw() {
    let pdw = this.validateForm.value.Pdw;
    if (pdw.length == 0) {
      this.validateForm.value.Province = undefined;
      this.validateForm.value.District = undefined;
      this.validateForm.value.Ward = undefined;
      this.getLaneData(undefined, false);
    }
    else {
      this.validateForm.get('Province')?.setValue(pdw[0]);
      this.validateForm.get('District')?.setValue(pdw[1]);
      this.validateForm.get('Ward')?.setValue(pdw[2]);
      this.getLaneData(pdw[2], false);
    }

    this.cdr.detectChanges();
  }

  close(): void {
    this.drawerRef.close();
  }
}
