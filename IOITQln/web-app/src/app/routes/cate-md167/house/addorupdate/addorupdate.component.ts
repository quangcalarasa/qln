import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PlanContent, TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167LandTaxRepository } from 'src/app/infrastructure/repositories/md167landtax.repository';
import { Md167PositionValueRepository } from 'src/app/infrastructure/repositories/md167positionvalue.repository';
import { NzModalService } from 'ng-zorro-antd/modal';
import { LandType, PurposeUsing, DecreeMd167 } from 'src/app/shared/utils/consts';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { GetByPageLandPriceModel } from 'src/app/core/models/get-by-page-model';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import { Md167LandPriceRepository } from 'src/app/infrastructure/repositories/md167landprice.repository';
import { HouseInfoComponent } from '../house-info/house-info.component';
import { HouseProposeComponent } from '../house-propose/house-propose.component';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';
import { roundIfDecimal } from 'src/app/shared/utils/common';
import { Md167StateOfUseRepository } from 'src/app/infrastructure/repositories/md167-state-of-use.repository';
import { Md167ManPurposeRepository } from 'src/app/infrastructure/repositories/md167-manage-purpose.repository';
import { KiosComponent } from '../kios/kios.component';
import { Md167TranferUnitRepository } from 'src/app/infrastructure/repositories/md167-tranfer-unit.repository';
import { AddorupdateMd167KiosComponent } from '../kios/addorupdate/addorupdate.component';
import { Md167PlanContentRepository } from 'src/app/infrastructure/repositories/md167-plan-content.repository';
import { Md167AreaValueRepository } from 'src/app/infrastructure/repositories/md167area-value.repository';
import { Md167FileRepository } from 'src/app/infrastructure/repositories/reportwordmd167.repository';

@Component({
  selector: 'app-addorupdate',
  templateUrl: './addorupdate.component.html',
  styles: [
  ]
})
export class AddorupdateHouseComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  pdw_data: any;
  unitPriceTotal = 0;
  valuePrice = 0;
  type_house_data: any;
  selectedTypeHouse: any;
  TypeQD = TypeQD;
  lstDataLandTax: any;
  lane_data: any;
  tranfer_unit_data: any;
  lstDecree = DecreeMd167;
  lstArea: any;
  landType = LandType;
  planContent: any;
  // areaValue: any;
  data_apart: any;
  land_tax_selected: any;
  data_house: any;
  nzFormat = 'dd/ MM/ yyyy';
  suffix: string = ' m2';
  myForm = new FormGroup({
    IsPayTax: new FormControl(false)
  });
  info_apartment = {
    KiosCount: 0,
    KiosUsed: 0,
    KiosEmpty: 0,
  };
  locationValueHouse: any = 0;
  areaValueHouse = 0;
  unitPrice = 0;
  lstLocation = [
    { Name: 'Vị trí 1', Id: 1 },
    { Name: 'Vị trí 2', Id: 2 },
    { Name: 'Vị trí 3', Id: 3 },
    { Name: 'Vị trí 4', Id: 4 },
  ]
  dataHouse = {
    LandTaxRate: 0,
    ApaValue: 0,
    UnitPriceValue: 0,
    ApaTax: 0,
  }
  stateOfUse: any;
  @ViewChild('houseInfoComponent') houseInfoComponent!: HouseInfoComponent;
  @ViewChild('houseProposeComponent') houseProposeComponent!: HouseProposeComponent;
  @ViewChild('kiosComponent') kiosComponent!: KiosComponent;
  locationValue: any;
  purposeUsing: any;
  @Input() record: NzSafeAny;

  landprice_data: any[] = [];

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private md167LandTaxRepository: Md167LandTaxRepository,
    private md167ManPurposeRepository: Md167ManPurposeRepository,
    private md167StateOfUseRepository: Md167StateOfUseRepository,
    private md167TranferUnitRepository: Md167TranferUnitRepository,
    private md167HouseTypeRepository: Md167HouseTypeRepository,
    private md167AreaValueRepository: Md167AreaValueRepository,
    private laneRepository: LaneRepository,
    private md167HouseRepository: Md167HouseRepository,
    private md167PlanContentRepository: Md167PlanContentRepository,
    private provinceRepository: ProvinceRepository,
    private md167PositionValueRepository: Md167PositionValueRepository,
    private md167LandPriceRepository: Md167LandPriceRepository,
    private md167FileRepository: Md167FileRepository,
    private cdr: ChangeDetectorRef,
    private modalSrv: NzModalService,) {

  }

  ngOnInit(): void {
    this.getLandTax();
    this.getCascaderData();
    if (this.record) {
      this.unitPriceTotal = this.record.UnitPriceValue
      this.getApartInfo(this.record.Id);
      this.locationValueHouse = this.record.LocationCoefficient
      this.areaValueHouse = this.record.AreaValue
      this.unitPrice = this.record.LandPrice
      this.dataHouse = {
        LandTaxRate: this.record.LandTaxRate,
        ApaValue: this.record.ApaValue,
        UnitPriceValue: this.record.UnitPriceValue ? this.record.UnitPriceValue : 0,
        ApaTax: this.record.ApaTax ? this.record.ApaTax : 0,
      }

      // console.log(this.dataHouse);
    }
    this.getDataClone();
    this.getData();
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      HouseNumber: [this.record ? this.record.HouseNumber : undefined, [Validators.required]],
      ProvinceId: [this.record ? this.record.ProvinceId : undefined, []],
      LandPrice: [this.record ? this.record.LandPrice : undefined, [Validators.required]],
      LandPriceItemId: [this.record ? this.record.LandPriceItemId : undefined, [Validators.required]],
      DistrictId: [this.record ? this.record.DistrictId : undefined, []],
      WardId: [this.record ? this.record.WardId : undefined, []],
      LaneId: [this.record ? this.record.LaneId : undefined, [Validators.required]],
      MapNumber: [this.record ? this.record.MapNumber : undefined, []],
      ParcelNumber: [this.record ? this.record.ParcelNumber : undefined, []],
      LandTaxRate: [this.record ? this.record.LandTaxRate : undefined, [Validators.required]],
      PlanningInfor: [this.record ? this.record.PlanningInfor : undefined, []],
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
      Md167TransferUnitId: [this.record ? this.record.Md167TransferUnitId : undefined, [Validators.required]],
      ReceptionDate: [this.record ? convertDate(this.record.ReceptionDate) : undefined, []],
      decree: [this.record ? this.record.decree : 99, [Validators.required]],
      Location: [this.record ? this.record.Location : undefined, [Validators.required]],
      HouseTypeId: [this.record ? this.record.HouseTypeId : undefined, [Validators.required]],
      LocationCoefficient: [{ value: this.record ? this.record.LocationCoefficient : undefined, disabled: true }, [Validators.required]],
      UnitPrice: [{ value: this.record ? this.record.UnitPrice : undefined, disabled: true }, [Validators.required]],
      UnitPriceValue: [this.record ? this.record.UnitPriceValue : undefined, []],
      SHNNCode: [this.record ? this.record.SHNNCode : undefined, []],
      SHNNDate: [this.record ? convertDate(this.record.SHNNDate) : undefined, []],
      ContractCode: [this.record ? this.record.ContractCode : undefined, []],
      ContractDate: [this.record ? convertDate(this.record.ContractDate) : undefined, []],
      LeaseCode: [this.record ? this.record.LeaseCode : undefined, []],
      LeaseDate: [this.record ? convertDate(this.record.LeaseDate) : undefined, []],
      LeaseCertCode: [this.record ? this.record.LeaseCertCode : undefined, []],
      LeaseCertDate: [this.record ? convertDate(this.record.LeaseCertDate) : undefined, []],
      Md167HouseId: [this.record ? this.record.Md167HouseId : undefined, []],
      PurposeUsing: [this.record ? this.record.PurposeUsing : undefined, [Validators.required]],
      DocumentCode: [this.record ? this.record.DocumentCode : undefined, []],
      DocumentDate: [this.record ? convertDate(this.record.DocumentDate) : undefined, []],
      PlanContent: [this.record ? this.record.PlanContent : undefined, [Validators.required]],
      OriginPrice: [this.record ? this.record.OriginPrice : undefined, []],
      ValueLand: [this.record ? this.record.ValueLand : undefined, []],
      TypeHouse: [this.record ? this.record.TypeHouse : 1, [Validators.required]],
      StatusOfUse: [this.record ? this.record.StatusOfUse : undefined, [Validators.required]],
      // AreaValueId: [this.record ? this.record.AreaValueId : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined, []],
      HouAreaLand: [this.record ? this.record.HouAreaLand : 0, []],
      TaxNN: [{ value: this.record ? this.record.TaxNN : 0, disabled: true }, []],
      UseFloorPb: [this.record ? this.record.UseFloorPb : 0, []],
      UseFloorPr: [this.record ? this.record.UseFloorPr : 0, []],
      UseLandPb: [this.record ? this.record.UseLandPb : 0, []],
      UseLandPr: [this.record ? this.record.UseLandPr : 0, []],
      AreBuildPb: [this.record ? this.record.AreBuildPb : 0, []],
      AreBuildPr: [this.record ? this.record.AreBuildPr : 0, []],
      TextureScale: [this.record ? this.record.TextureScale : 0, []],
      AreaLandInSafe: [this.record ? this.record.AreaLandInSafe : 0, []],
      AreaLandInBankSafe: [this.record ? this.record.AreaLandInBankSafe : 0, []],
      AreaHouseInSafe: [this.record ? this.record.AreaHouseInSafe : 0, []],
      AreaHouseInBankSafe: [this.record ? this.record.AreaHouseInBankSafe : 0, []],
      ApaFloorCount: [this.record ? this.record.ApaFloorCount : 0, []],
      AreaTunnel: [this.record ? this.record.AreaTunnel : 0, []],
      ApaTax: [this.record ? this.record.ApaTax : 0, []],
      ApaIsBasement: [this.record ? this.record.ApaIsBasement : undefined, []],
      ApaValue: [this.record ? this.record.ApaValue : 0, []],
      KiosStatus: [this.record ? this.record.KiosStatus : undefined, []],
      AreaFloorBuild: [this.record ? this.record.AreaFloorBuild : 0, []],
      KiosCount: [{ value: 0, disabled: true }, []],
      KiosUsed: [{ value: 0, disabled: true }, []],
      KiosEmpty: [{ value: 0, disabled: true }, []],
      md167HouseProposes: [this.record ? this.record.md167HouseProposes : undefined, []],
      md167HouseInfos: [this.record ? this.record.md167HouseInfos : undefined, []],
      md167Kios: [this.record ? this.record.md167Kios : undefined, []],
      Pdw: [this.record ? [this.record.ProvinceId, this.record.DistrictId, this.record.WardId] : undefined, []],
      IsPayTax: [this.record ? this.record.IsPayTax : undefined, []],

    });

    this.GetLandPriceData(true);

    if (this.record) {
      // this.validateForm.get('UnitPrice')?.setValue(this.stringUnitPrice(this.locationValueHouse, this.unitPrice, this.areaValueHouse));
      this.getLaneData(this.record.WardId, true);
      if (this.validateForm.value.TypeHouse == 2) {
        this.validateForm.get('HouAreaLand')?.setValidators(null);
        this.validateForm.get('AreBuildPb')?.setValidators(null);
        this.validateForm.get('AreBuildPr')?.setValidators(null);

        this.validateForm.get('AreaFloorBuild')?.setValidators([Validators.required]);
        this.validateForm.get('ApaTax')?.setValidators([Validators.required]);
        this.validateForm.get('ApaIsBasement')?.setValidators([Validators.required]);

        this.validateForm.get('AreaFloorBuild')?.updateValueAndValidity();
        this.validateForm.get('ApaTax')?.updateValueAndValidity();
        this.validateForm.get('ApaIsBasement')?.updateValueAndValidity();

      }
    }
  }

  checkRequiredFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(controlName => {
      const control: any = formGroup.get(controlName);

      if (control.validator && control.validator({} as AbstractControl)?.required) {
        console.log(`Field '${controlName}' is required and '${control.value}'`);
      }

      if (control instanceof FormGroup) {
        this.checkRequiredFields(control);
      }
    });
  }

  stringUnitPrice(locationCoefficient: any, unitPriceValue: any, areaValue: any) {
    // this.unitPriceTotal = Math.round(unitPriceValue * locationCoefficient * areaValue);
    this.unitPriceTotal = Math.round(unitPriceValue * locationCoefficient * areaValue);
    this.valuePrice = Math.round(unitPriceValue);
    console.log('test',this.valuePrice);
    
    this.changeTaxNN();
    this.dataHouse.UnitPriceValue = this.valuePrice;

    return `${unitPriceValue.toLocaleString('de-DE', { useGrouping: true, maximumFractionDigits: 0 })}` + " đ/m² x " + areaValue + " x " + locationCoefficient +
      ` = ${(Math.round(unitPriceValue * locationCoefficient * areaValue)).toLocaleString('de-DE', { useGrouping: true, maximumFractionDigits: 0 })} đ/m²`;
  }

  async selectLandTax() {
    if (this.validateForm.value.LandTaxRate != null) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.select = 'Tax';
      paging.query = `Id=${this.validateForm.value.LandTaxRate}`
      const resp = await this.md167LandTaxRepository.getByPage(paging);

      if (resp.meta?.error_code == 200) {
        this.land_tax_selected = resp.data[0].Tax;
        this.dataHouse.LandTaxRate = this.land_tax_selected;
        this.changeTaxNN()
      } else {
        this.modalSrv.error({
          nzTitle: 'Không Lấy Được Dữ Liệu!!!'
        });
      }
    }
    this.dataHouse.LandTaxRate = this.validateForm.value.LandTaxRate
  }

  changeTaxNN() {
    let value = 0;
    if (this.validateForm.value.UseLandPb != undefined && this.validateForm.value.UseLandPr != undefined
      && this.valuePrice != undefined && this.land_tax_selected != undefined)
      console.log(this.valuePrice);
      
      value = (this.validateForm.value.UseLandPb + this.validateForm.value.UseLandPr) * (this.valuePrice * 80/100) * this.land_tax_selected / 100;
    this.validateForm.get('TaxNN')?.setValue(value);
  }

  addApaTax() {
    this.dataHouse.ApaTax = this.validateForm.value.ApaTax;
  }

  getDataClone() {
    if (this.record) {
      if (this.record.TypeHouse == 1) {
        this.data_house = {
          HouAreaLand: this.record.HouAreaLand,
          TaxNN: this.record.TaxNN,
          UseFloorPb: this.record.UseFloorPb,
          UseFloorPr: this.record.UseFloorPr,
          UseLandPb: this.record.UseLandPb,
          UseLandPr: this.record.UseLandPr,
          AreBuildPb: this.record.AreBuildPb,
          AreBuildPr: this.record.AreBuildPr,
          AreaLandInSafe: this.record.AreaLandInSafe,
          AreaHouseInSafe: this.record.AreaHouseInSafe,
          AreaHouseInBankSafe: this.record.AreaHouseInBankSafe,
          AreaLandInBankSafe: this.record.AreaLandInBankSafe,
        }
        this.data_apart = {
          AreaFloorBuild: 0,
          AreaTunnel: 0,
          UseFloorPb: 0,
          UseFloorPr: 0,
          UseLandPb: 0,
          UseLandPr: 0,
          AreaLandInSafe: 0,
          AreaHouseInSafe: 0,
          AreaHouseInBankSafe: 0,
          AreaLandInBankSafe: 0,
          ApaFloorCount: 0,
          ApaTax: 1,
          ApaIsBasement: false,
          ApaValue: 1,
        }
      }
      else {
        this.data_apart = {
          AreaFloorBuild: this.record.AreaFloorBuild,
          AreaTunnel: this.record.AreaTunnel,
          UseFloorPb: this.record.UseFloorPb,
          UseFloorPr: this.record.UseFloorPr,
          UseLandPb: this.record.UseLandPb,
          UseLandPr: this.record.UseLandPr,
          AreaLandInSafe: this.record.AreaLandInSafe,
          AreaHouseInSafe: this.record.AreaHouseInSafe,
          AreaHouseInBankSafe: this.record.AreaHouseInBankSafe,
          AreaLandInBankSafe: this.record.AreaLandInBankSafe,
          ApaFloorCount: this.record.ApaFloorCount,
          ApaTax: this.record.ApaTax,
          ApaIsBasement: this.record.ApaIsBasement,
          ApaValue: this.record.ApaValue,
        }
        this.data_house = {
          HouAreaLand: 0,
          TaxNN: 0,
          UseFloorPb: 0,
          UseFloorPr: 0,
          UseLandPb: 0,
          UseLandPr: 0,
          AreBuildPb: 0,
          AreBuildPr: 0,
          AreaLandInSafe: 0,
          AreaHouseInSafe: 0,
          AreaHouseInBankSafe: 0,
          AreaLandInBankSafe: 0,
        }
      }
    }
  }
  check() {
    console.log(this.validateForm.value.md167Kios);
    console.log(this.validateForm.value.md167HouseInfos);
    console.log(this.validateForm.value.md167HouseProposes);

  }
  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';

    const resp1 = await this.md167HouseTypeRepository.getByPage(paging);

    if (resp1.meta?.error_code == 200) {
      this.type_house_data = resp1.data;
    }

    const resp2 = await this.md167ManPurposeRepository.getByPage(paging);

    if (resp2.meta?.error_code == 200) {
      this.purposeUsing = resp2.data;
    }

    const resp3 = await this.md167StateOfUseRepository.getByPage(paging);

    if (resp3.meta?.error_code == 200) {
      this.stateOfUse = resp3.data;
    }

    const resp4 = await this.md167TranferUnitRepository.getByPage(paging);

    if (resp4.meta?.error_code == 200) {
      this.tranfer_unit_data = resp4.data;
    }
    const resp5 = await this.md167PlanContentRepository.getByPage(paging);

    if (resp5.meta?.error_code == 200) {
      this.planContent = resp5.data;
    }

    // const resp6 = await this.md167AreaValueRepository.getByPage(paging);

    // if (resp6.meta?.error_code == 200) {
    //   this.areaValue = resp6.data;
    // }
  }

  changeValueKios() {
    this.validateForm.get('md167Kios')?.setValue([...this.kiosComponent.getValue()]);
  }

  setValueChangeType(check: boolean) {
    if (check) {
      this.data_apart.AreaFloorBuild = this.validateForm.value.AreaFloorBuild;
      this.data_apart.AreaTunnel = this.validateForm.value.AreaTunnel;
      this.data_apart.UseFloorPb = this.validateForm.value.UseFloorPb;
      this.data_apart.UseFloorPr = this.validateForm.value.UseFloorPr;
      this.data_apart.UseLandPb = this.validateForm.value.UseLandPb;
      this.data_apart.UseLandPr = this.validateForm.value.UseLandPr;
      this.data_apart.AreaLandInSafe = this.validateForm.value.AreaLandInSafe;
      this.data_apart.AreaHouseInSafe = this.validateForm.value.AreaHouseInSafe;
      this.data_apart.AreaLandInBankSafe = this.validateForm.value.AreaLandInBankSafe;
      this.data_apart.AreaHouseInBankSafe = this.validateForm.value.AreaHouseInBankSafe;
      this.data_apart.ApaFloorCount = this.validateForm.value.ApaFloorCount;
      this.data_apart.ApaTax = this.validateForm.value.ApaTax;
      this.data_apart.ApaIsBasement = this.validateForm.value.ApaIsBasement;
      this.data_apart.ApaValue = this.validateForm.value.ApaValue;

      this.validateForm.get('HouAreaLand')?.setValue(this.data_house.HouAreaLand);
      this.validateForm.get('TaxNN')?.setValue(this.data_house.TaxNN);
      this.validateForm.get('UseFloorPb')?.setValue(this.data_house.UseFloorPb);
      this.validateForm.get('UseFloorPr')?.setValue(this.data_house.UseFloorPr);
      this.validateForm.get('UseLandPb')?.setValue(this.data_house.UseLandPb);
      this.validateForm.get('UseLandPr')?.setValue(this.data_house.UseLandPr);
      this.validateForm.get('AreBuildPb')?.setValue(this.data_house.AreBuildPb);
      this.validateForm.get('AreBuildPr')?.setValue(this.data_house.AreBuildPr);
      this.validateForm.get('AreaLandInSafe')?.setValue(this.data_house.AreaLandInSafe);
      this.validateForm.get('AreaHouseInSafe')?.setValue(this.data_house.AreaHouseInSafe);
      this.validateForm.get('AreaHouseInBankSafe')?.setValue(this.data_house.AreaHouseInBankSafe);
      this.validateForm.get('AreaLandInBankSafe')?.setValue(this.data_house.AreaLandInBankSafe);

      this.validateForm.get('AreaFloorBuild')?.setValidators(null);
      this.validateForm.get('ApaTax')?.setValidators(null);
      this.validateForm.get('ApaIsBasement')?.setValidators(null);

      this.validateForm.get('AreBuildPr')?.setValidators([Validators.required]);
      this.validateForm.get('AreBuildPb')?.setValidators([Validators.required]);
      this.validateForm.get('HouAreaLand')?.setValidators([Validators.required]);

      this.validateForm.get('AreBuildPr')?.updateValueAndValidity();
      this.validateForm.get('AreBuildPb')?.updateValueAndValidity();
      this.validateForm.get('HouAreaLand')?.updateValueAndValidity();

    }
    else {
      this.data_house.HouAreaLand = this.validateForm.value.HouAreaLand;
      this.data_house.TaxNN = this.validateForm.value.TaxNN;
      this.data_house.UseFloorPb = this.validateForm.value.UseFloorPb;
      this.data_house.UseFloorPr = this.validateForm.value.UseFloorPr;
      this.data_house.UseLandPb = this.validateForm.value.UseLandPb;
      this.data_house.UseLandPr = this.validateForm.value.UseLandPr;
      this.data_house.AreBuildPb = this.validateForm.value.AreBuildPb;
      this.data_house.AreBuildPr = this.validateForm.value.AreBuildPr;
      this.data_house.AreaLandInSafe = this.validateForm.value.AreaLandInSafe;
      this.data_house.AreaHouseInSafe = this.validateForm.value.AreaHouseInSafe;
      this.data_house.AreaHouseInBankSafe = this.validateForm.value.AreaHouseInBankSafe;
      this.data_house.AreaLandInBankSafe = this.validateForm.value.AreaLandInBankSafe;

      this.validateForm.get('AreaFloorBuild')?.setValue(this.data_apart.AreaFloorBuild);
      this.validateForm.get('AreaTunnel')?.setValue(this.data_apart.AreaTunnel);
      this.validateForm.get('UseFloorPb')?.setValue(this.data_apart.UseFloorPb);
      this.validateForm.get('UseFloorPr')?.setValue(this.data_apart.UseFloorPr);
      this.validateForm.get('UseLandPb')?.setValue(this.data_apart.UseLandPb);
      this.validateForm.get('UseLandPr')?.setValue(this.data_apart.UseLandPr);
      this.validateForm.get('AreaLandInSafe')?.setValue(this.data_apart.AreaLandInSafe);
      this.validateForm.get('AreaHouseInSafe')?.setValue(this.data_apart.AreaHouseInSafe);
      this.validateForm.get('AreaHouseInBankSafe')?.setValue(this.data_apart.AreaHouseInBankSafe);
      this.validateForm.get('AreaLandInBankSafe')?.setValue(this.data_apart.AreaLandInBankSafe);
      this.validateForm.get('ApaFloorCount')?.setValue(this.data_apart.ApaFloorCount);
      this.validateForm.get('ApaTax')?.setValue(this.data_apart.ApaTax);
      this.validateForm.get('ApaIsBasement')?.setValue(this.data_apart.ApaIsBasement);
      this.validateForm.get('ApaValue')?.setValue(this.data_apart.ApaValue);

      this.validateForm.get('HouAreaLand')?.setValidators(null);
      this.validateForm.get('AreBuildPb')?.setValidators(null);
      this.validateForm.get('AreBuildPr')?.setValidators(null);

      this.validateForm.get('AreaFloorBuild')?.setValidators([Validators.required]);
      this.validateForm.get('ApaTax')?.setValidators([Validators.required]);
      this.validateForm.get('ApaIsBasement')?.setValidators([Validators.required]);

      this.validateForm.get('AreaFloorBuild')?.updateValueAndValidity();
      this.validateForm.get('ApaTax')?.updateValueAndValidity();
      this.validateForm.get('ApaIsBasement')?.updateValueAndValidity();
    }
  }

  async getLandTax() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Code,IsDefault,Tax';

    const resp = await this.md167LandTaxRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.lstDataLandTax = resp.data;
      this.lstDataLandTax.forEach((item: any) => {
        if (item.IsDefault) {
          this.validateForm.get('LandTaxRate')?.setValue(item.Id);
          this.land_tax_selected = item.Tax
          this.dataHouse.LandTaxRate = this.land_tax_selected;
        }
      });
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  setApaValue() {
    let areaTunnel = 0;
    this.validateForm.get('ApaIsBasement')?.setValue(!this.validateForm.value.ApaIsBasement)
    if (this.validateForm.value.ApaIsBasement == true) {
      areaTunnel = this.validateForm.value.AreaTunnel / 2;
    }
    let value = (this.validateForm.value.UseLandPb + this.validateForm.value.UseLandPr) / (this.validateForm.value.UseFloorPr + areaTunnel)
    this.validateForm.get('ApaValue')?.setValue(roundIfDecimal(value, 3));
    this.dataHouse.ApaValue = this.validateForm.value.ApaValue;
  }

  async getAreaValue() {
    const areaValue = await this.md167HouseRepository.getAreaValueHouse(this.validateForm.value.DistrictId)
    this.areaValueHouse = areaValue.data
    this.GetLandPriceData(false);
  }

  async changeLocation() {
    let decreeFilter = this.validateForm.value.decree;
    let location = this.validateForm.value.Location;

    if (decreeFilter && location) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 1;
      let LocationCoefficient = "";
      let LocationCoefficientValue: number = 1;
      paging.query = `decree=${decreeFilter}`;

      const resp = await this.md167PositionValueRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.locationValue = resp.data[0];

        if (this.locationValue) {
          if (location == 1) {
            LocationCoefficient = this.locationValue.Position1;
            LocationCoefficientValue = this.locationValue.Position1;
          } else if (location == 2) {
            LocationCoefficient = this.locationValue.Position2;
            LocationCoefficientValue = this.locationValue.Position2;
          }
          else if (location == 3) {
            LocationCoefficient = this.locationValue.Position3;
            LocationCoefficientValue = this.locationValue.Position3;
          }
          else if (location == 4) {
            LocationCoefficient = this.locationValue.Position4;
            LocationCoefficientValue = this.locationValue.Position4;
          }

          this.validateForm.get('LocationCoefficient')?.setValue(LocationCoefficient);
          // this.locationValueHouse = LocationCoefficient
          // this.validateForm.get('UnitPrice')?.setValue(this.stringUnitPrice(this.locationValueHouse, this.unitPrice, this.areaValueHouse));
          this.GenUnitPrice();
        }
        else {
          this.validateForm.get('LocationCoefficient')?.setValue(undefined);
          this.GenUnitPrice();
        }
      }
      else {
        this.validateForm.get('LocationCoefficient')?.setValue(undefined);
        this.GenUnitPrice();
      }
    }
    else {
      this.validateForm.get('LocationCoefficient')?.setValue(undefined);
      this.GenUnitPrice();
    }

  }

  async changeDecree() {
    this.validateForm.get('LocationCoefficient')?.setValue(undefined);
    this.GenUnitPrice();
    // if (this.validateForm.value.DistrictId != undefined && this.validateForm.value.LocationCoefficient != "") {
    //   let data: any
    //   let pagingLandPrice: GetByPageLandPriceModel = new GetByPageLandPriceModel();
    //   pagingLandPrice.query = `District=${this.validateForm.value.DistrictId}`;
    //   pagingLandPrice.order_by = 'CreatedAt Desc';
    //   pagingLandPrice.landPriceType = 2;
    //   const resp = await this.md167LandPriceRepository.getByPage(pagingLandPrice);
    //   if (resp.meta?.error_code == 200) {
    //     console.log(resp.data);
    //   }
    // }
  }

  async loadValueLocation() {
    let priceValue: any
    if (this.validateForm.value.LaneId != undefined && this.validateForm.value.LaneId != null)
      priceValue = await this.md167HouseRepository.getPriceHouse(this.validateForm.value.DistrictId, this.validateForm.value.LaneId)
    this.unitPrice = priceValue.data
    this.validateForm.get('UnitPrice')?.setValue(this.stringUnitPrice(this.locationValueHouse, this.unitPrice, this.areaValueHouse));
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
      this.validateForm.get('ProvinceId')?.setValue(pdw[0]);
      this.validateForm.get('DistrictId')?.setValue(pdw[1]);
      this.validateForm.get('WardId')?.setValue(pdw[2]);
      this.getLaneData(pdw[2], false);
    }

    this.cdr.detectChanges();
    this.getAreaValue();
  }

  changeHousePropose() {
    this.validateForm.get('md167HouseProposes')?.setValue([...this.houseProposeComponent.getValue()]);
  }

  changeHouseInfo() {
    this.validateForm.get('md167HouseInfos')?.setValue([...this.houseInfoComponent.getValue()]);
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

  async getApartInfo(id: any) {
    if (typeof id === 'number') {
      try {
        this.loading = true;
        const resp = await this.md167HouseRepository.GetInfoApartment(id);
        if (resp.meta?.error_code == 200) {
          this.validateForm.get("KiosCount")?.setValue(resp.data.KiosCount)
          this.validateForm.get("KiosUsed")?.setValue(resp.data.KiosUsed)
          this.validateForm.get("KiosEmpty")?.setValue(resp.data.KiosEmpty)
        }
      } catch (error) {
        throw error;
      } finally {
        this.loading = false;
      }
    }
  }

  changeTypeHouse() {
    if (this.validateForm.value.TypeHouse == 1) {
      this.validateForm.get('TypeHouse')?.setValue(2);
      if (this.record) this.setValueChangeType(false);
    }
    else {
      this.validateForm.get('TypeHouse')?.setValue(1);
      if (this.record) this.setValueChangeType(true);
    }
  }

  async getLaneData(wardId?: number, init: boolean = true) {
    if (!init) this.validateForm.get('LaneId')?.setValue(undefined);
    this.lane_data = [];
    if (!wardId) return;

    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Ward=${wardId}`;

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_data = resp.data;

      if (resp.metadata == 1 && !init) {
        this.validateForm.get('LaneId')?.setValue(this.lane_data[0].Id);
      }
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    data.md167HouseInfos = [...this.houseInfoComponent.getValue()];
    data.md167HouseProposes = [...this.houseProposeComponent.getValue()];
    data.UnitPriceValue = this.unitPriceTotal;
    data.LandPrice = this.unitPrice;

    const isIsPayTax = this.validateForm.get('IsPayTax')?.value;

    data.isIsPayTax = isIsPayTax;

    const resp = data.Id ? await this.md167HouseRepository.update(data) : await this.md167HouseRepository.addNew(data);
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

  async GetLandPriceData(init: boolean) {
    if (!init) {
      this.validateForm.get('UnitPrice')?.setValue(undefined);
      this.validateForm.get('LandPrice')?.setValue(undefined);
      this.validateForm.get('LandPriceItemId')?.setValue(undefined);
    }

    let district = this.validateForm.get('DistrictId')?.value;
    let lane = this.validateForm.get('LaneId')?.value;
    let decree = this.validateForm.get('decree')?.value;

    this.landprice_data = [];

    if (district && lane && decree) {
      const resp = await this.md167HouseRepository.GetLandPriceData(district, lane, decree);
      if (resp.meta?.error_code == 200) {
        this.landprice_data = resp.data;
      }
    }
  }

  GenUnitPrice() {
    let locationValueHouse = this.validateForm.get('LocationCoefficient')?.value;
    let landPriceItemId = this.validateForm.get('LandPriceItemId')?.value;

    if (landPriceItemId) {
      let landPriceItem = this.landprice_data.find(x => x.Id == landPriceItemId);

      if (locationValueHouse && landPriceItem && this.areaValueHouse) {
        this.validateForm.get('LandPrice')?.setValue(landPriceItem.Value);
        this.validateForm.get('UnitPrice')?.setValue(this.stringUnitPrice(locationValueHouse, landPriceItem.Value, this.areaValueHouse));
      }
      else {
        this.validateForm.get('LandPrice')?.setValue(undefined);
        this.validateForm.get('UnitPrice')?.setValue(undefined);
      }
    }
    else {
      this.validateForm.get('LandPrice')?.setValue(undefined);
      this.validateForm.get('UnitPrice')?.setValue(undefined);
    }
  }

  onClick(){
    this.md167FileRepository.GetExportContract5(this.record.Id);
  }
}