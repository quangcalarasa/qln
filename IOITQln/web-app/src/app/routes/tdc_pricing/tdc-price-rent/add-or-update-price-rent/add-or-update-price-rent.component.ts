import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';
import { convertMoney } from 'src/app/infrastructure/utils/common';

import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { TdcCustomerRepository } from 'src/app/infrastructure/repositories/tdcCustomer.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TdcPriceRentTaxComponent } from '../tdc-price-rent-tax/tdc-price-rent-tax.component';
import { TdcPriceRentTemporaryComponent } from '../tdc-price-rent-temporary/tdc-price-rent-temporary.component';
import { TdcPriceRentOfficialComponent } from '../tdc-price-rent-official/tdc-price-rent-official.component';
import { ShowLogTemporaryComponent } from '../show-log-temporary/show-log-temporary.component';

@Component({
  selector: 'app-add-or-update-price-rent',
  templateUrl: './add-or-update-price-rent.component.html'
})
export class AddOrUpdatePriceRentComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @ViewChild('TdcPriceRentTaxComponent') private TdcPriceRentTaxComponent!: TdcPriceRentTaxComponent;
  @ViewChild('TdcPriceRentTemporaryComponent') private TdcPriceRentTemporaryComponent: TdcPriceRentTemporaryComponent;
  @ViewChild('TdcPriceRentOfficialComponent') private TdcPriceRentOfficialComponent: TdcPriceRentOfficialComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;

  selectCorner: any;
  tdcCustomer_data: any[] = [];
  tdcProject_data: any[] = [];
  tdcLand_data = [];
  tdcBlockHouse_data = [];
  tdcFloor_data = [];
  tdcApartment_data = [];
  tdcProject_Filter_data: any[] = [];
  data_landTDC_filter: NzSafeAny = [];
  data_BlockHouseTDC_filter: NzSafeAny = [];
  data_FloorTDC_filter: NzSafeAny = [];
  data_ApartmentTDC_filter: NzSafeAny = [];

  tempTotalArea = 0;
  tempTotalPrice = 0;

  offiTotalArea = 0;
  offiTotalPrice = 0;

  priceMonth = 0;
  priceTC = 0;

  check = false;
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private tdcPriceRentRepository: TdcPriceRentRepository,
    private modalSrv: NzModalService,
    private tdcCustomerRepository: TdcCustomerRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository
  ) {}

  ngOnInit(): void {
    if (localStorage.getItem('add') == 'false') this.add = false;
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined],
      Date: [this.record ? convertDate(this.record.Date) : undefined, [Validators.required]],
      Floor1: [this.record ? this.record.Floor1 : undefined],
      TdcCustomerId: [this.record ? this.record.TdcCustomerId : undefined, [Validators.required]],
      TdcProjectId: [this.record ? this.record.TdcProjectId : undefined, [Validators.required]],
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined, [Validators.required]],
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined, [Validators.required]],
      TdcApartmentId: [this.record ? this.record.TdcApartmentId : undefined, [Validators.required]],
      DateTDC: [this.record ? convertDate(this.record.DateTDC) : undefined, [Validators.required]],
      MonthRent: [this.record ? this.record.MonthRent : undefined, [Validators.required]],
      PriceTC: [{ value: this.record ? this.record.PriceTC : 0, disabled: true }],
      PriceMonth: [{ value: this.record ? this.record.PriceMonth : 0, disabled: true }],
      PriceToTal: [this.record ? this.record.PriceToTal : undefined],
      PriceTT: [this.record ? this.record.PriceTT : undefined],
      DateTTC: [this.record ? convertDate(this.record.DateTTC) : undefined, [Validators.required]],
      tdcPriceRentTaxes: [this.record ? this.record.tdcPriceRentTaxes : [], []],
      tdcPriceRentTemporaries: [this.record ? this.record.tdcPriceRentTemporaries : []],
      tdcPriceRentOfficials: [this.record ? this.record.tdcPriceRentOfficials : [], []],
      Corner: [this.record ? this.record.Corner : false],
      DecisionNumberTT: [this.record ? this.record.DecisionNumberTT : undefined],
      DecisionDateTT: [this.record ? convertDate(this.record.DecisionDateTT) : undefined],
      DecisionNumberCT: [this.record ? this.record.DecisionNumberCT : undefined],
      DecisionDateCT: [this.record ? convertDate(this.record.DecisionDateCT) : undefined, [Validators.required]],
      TotalPriceTT: [{ value: this.record ? this.record.TotalPriceTT : 0, disabled: true }],
      TotalPriceCT: [{ value: this.record ? this.record.TotalPriceCT : 0, disabled: true }],
      TotalAreaTT: [{ value: this.record ? this.record.TotalAreaTT : 0, disabled: true }],
      TotalAreaCT: [{ value: this.record ? this.record.TotalAreaCT : 0, disabled: true }]
    });

    this.getDataCustomerTDC();
    this.getDataProjectTDC();

    if (this.record) {
      this.check = this.validateForm.value.Corner;
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataBlockHouseTDC(this.record.LandId);
      this.getDataFloorTDC(this.record.BlockHouseId);
      this.getApartmentTDC(this.record.FloorTdcId);
    }
  }

  async submitForm() {
    this.loading = true;
    this.validateForm.get('tdcPriceRentTemporaries')?.setValue(this.TdcPriceRentTemporaryComponent.tableItemRef._data);
    this.validateForm.get('tdcPriceRentOfficials')?.setValue(this.TdcPriceRentOfficialComponent.tableItemRef._data);
    let data = { ...this.validateForm.getRawValue() };

    data.tdcPriceRentTaxes = this.TdcPriceRentTaxComponent.getValue();
    data.Corner = this.check;

    const resp = data.Id ? await this.tdcPriceRentRepository.update(data) : await this.tdcPriceRentRepository.addNew(data);
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

  async getDataCustomerTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Code,FullName';

    const resp = await this.tdcCustomerRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcCustomer_data = resp.data;
    } 
  }
  async getDataProjectTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = "Id,Name";
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_data = resp.data;
    } 
  }
  // Giá bán cấu thành tạm thời
  SetIngrePriceTemp() {
    const newArray: { IngredientsPriceId: number; IngrePriceName: string; Area: number; Price: number; Total: number }[] =
      this.tdcProject_Filter_data.map(x => ({
        IngrePriceName: x.IngrePriceName,
        IngredientsPriceId: x.IngredientsPriceId,
        Area: 0,
        Price: 0,
        Total: 0
      }));
    this.validateForm.get('tdcPriceRentTemporaries')?.setValue(newArray);
  }

  async SetIngrePrice(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    paging.select = '';
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_Filter_data = resp.data[0].tDCProjectIngrePrices;
    }
    this.SetIngrePriceTemp();
    this.SetIngrePriceOffi();
  }

  //Giá bán cấu thành Chính Thức
  SetIngrePriceOffi() {
    const newArray: { IngredientsPriceId: number; IngrePriceName: string; Area: number; Price: number; Total: number }[] =
      this.tdcProject_Filter_data.map(x => ({
        IngrePriceName: x.IngrePriceName,
        IngredientsPriceId: x.IngredientsPriceId,
        Area: 0,
        Price: 0,
        Total: 0
      }));
    this.validateForm.get('tdcPriceRentOfficials')?.setValue(newArray);
  }

  ////Lấy Dữ Liệu Lô
  async getDataLandTDC(tdcProjectId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TdcProjectId=${tdcProjectId}`;
    paging.select = 'Id,Code,Name,TdcProjectId';

    const resp = await this.landRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcLand_data = resp.data;
      this.data_landTDC_filter = this.tdcLand_data;
    } 
  }

  async TDCLandByProject(event: number) {
    if (event) {
      this.getDataLandTDC(event);
      this.SetIngrePrice(event);
    }
  }

  ///Lấy Dữ Liệu Khối
  async getDataBlockHouseTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId';

    const resp = await this.blockHouseRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcBlockHouse_data = resp.data;
      this.data_BlockHouseTDC_filter = this.tdcBlockHouse_data;
    } 
  }
  TDCBlockHouseByLand(event: number) {
    if (event) {
      this.getDataBlockHouseTDC(event);
    }
  }

  /// Lấy Dữ Liệu Tầng
  async getDataFloorTDC(BlockHouseId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `BlockHouseId=${BlockHouseId}`;
    paging.select = 'Id,Code,Name,BlockHouseId,FloorNumber';

    const resp = await this.floorTdcRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcFloor_data = resp.data;
      this.data_FloorTDC_filter = this.tdcFloor_data;
    } 
  }
  TDCFloorByBlockHouse(event: number) {
    if (event) {
      this.getDataFloorTDC(event);
    }
  }
  ///Lấy Dữ Liệu Căn
  async getApartmentTDC(FloorTdcId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `FloorTdcId=${FloorTdcId}`;
    paging.select = 'Id,Code,Name,FloorTdcId,Corner';

    const resp = await this.apartmentTdcRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcApartment_data = resp.data;
      this.data_ApartmentTDC_filter = this.tdcApartment_data;
    } 
  }
  TDCApartmentrByFloor(event: any) {
    if (event) {
      this.getApartmentTDC(event);
      const selectedOption = this.data_FloorTDC_filter.find((option: any) => option.Id === event);
      this.validateForm.get('Floor1')?.setValue(this.calcFloor1(selectedOption));
    }
  }
  calcFloor1(data: any) {
    if (data.FloorNumber > 1) {
      return data.FloorNumber - 1;
    } else {
      return 'G';
    }
  }
  TdcCornerByApartment(event: any) {
    if (event) {
      this.selectCorner = this.data_ApartmentTDC_filter.find((i: any) => i.Id === event);
      if (this.selectCorner.Corner == true) this.check = true;
      else this.check = false;
    }
  }
  //Thành Phần Giá Bán Tạm Thời
  ChangeTotalTemp(event: any) {
    let area = 0;
    let total = 0;
    for (let i = 0; i < event.length; i++) {
      area += event[i].Area;
      total += event[i].Total;
    }
    this.tempTotalArea = area;
    this.tempTotalPrice = total;

    this.validateForm.get('TotalAreaTT')?.setValue(this.tempTotalArea);
    this.validateForm.get('TotalPriceTT')?.setValue(this.tempTotalPrice);
  }

  //Thành Phần Giá Bán Chính Thức
  ChangeTotalOffi(event: any) {
    let area = 0;
    let total = 0;
    for (let i = 0; i < event.length; i++) {
      area += event[i].Area;
      total += event[i].Total;
    }
    this.offiTotalArea = area;
    this.offiTotalPrice = total;

    this.validateForm.get('TotalAreaCT')?.setValue(this.offiTotalArea);
    this.validateForm.get('TotalPriceCT')?.setValue(this.offiTotalPrice);
  }

  ChangeMonthRent() {
    this.changePriceMonth();
    return this.validateForm.value.MonthRent;
  }

  changePriceMonth() {
    if (this.validateForm.value.MonthRent) {
      if (this.offiTotalPrice > 0) {
        this.priceMonth = this.offiTotalPrice / this.validateForm.value.MonthRent;
        this.changePriceTC(this.priceMonth);
        return this.validateForm.get('PriceMonth')?.setValue(this.priceMonth);
      } else {
        if (this.tempTotalPrice > 0) {
          this.priceMonth = this.tempTotalPrice / this.validateForm.value.MonthRent;
          this.changePriceTC(this.priceMonth);
          return this.validateForm.get('PriceMonth')?.setValue(this.priceMonth);
        }
      }
    }
  }

  changePriceTC(env: any) {
    this.priceTC = 12 * env;
    return this.validateForm.get('PriceTC')?.setValue(this.priceTC);
  }

  showLog() {
    this.modalSrv.create({
      nzTitle: `Lịch sử thay đổi thành phần giá bán cấu thành chính thức`,
      nzWidth: '75vw',
      nzContent: ShowLogTemporaryComponent,
      nzComponentParams: {
        Id: this.validateForm.value.Id,
        type:1
      }
    });
  }
}
