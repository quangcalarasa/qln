import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate, convertMoney } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { PlatformTdcRepository } from 'src/app/infrastructure/repositories/platform-tdc.repository';
import { TdcCustomerRepository } from 'src/app/infrastructure/repositories/tdcCustomer.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TdcPriceOneSellTemporaryComponent } from '../tdc-price-one-sell-temporary/tdc-price-one-sell-temporary.component';
import { TdcPriceOneSellOfficialComponent } from '../tdc-price-one-sell-official/tdc-price-one-sell-official.component'; 
import { TdcPriceOneSellRepository } from 'src/app/infrastructure/repositories/tdcPriceOneSell.repository';
import { TdcPriceOneSellTaxComponent } from '../tdc-price-one-sell-tax/tdc-price-one-sell-tax.component';
import { ShowLogOfficialComponent } from '../show-log-official/show-log-official.component';

@Component({
  selector: 'app-add-or-update-tdc-price-one-sell',
  templateUrl: './add-or-update-tdc-price-one-sell.component.html',
  styles: [
  ]
})
export class AddOrUpdateTdcPriceOneSellComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @ViewChild('TdcPriceOneSellTaxComponent') private TdcPriceOneSellTaxComponent: TdcPriceOneSellTaxComponent;
  @ViewChild('TdcPriceOneSellTemporaryComponent') private TdcPriceOneSellTemporaryComponent: TdcPriceOneSellTemporaryComponent;
  @ViewChild('TdcPriceOneSellOfficialComponent') private TdcPriceOneSellOfficialComponent: TdcPriceOneSellOfficialComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;
  @Input() tdcPriceOneSell: any[] = [];
  tdcCustomer_data: any[] = [];
  tdcProject_data: any[] = [];
  tdcLand_data = [];
  tdcBlockHouse_data = [];
  tdcPlatform_data = [];
  tdcFloor_data = [];
  tdcApartment_data = [];
  tdcProject_Filter_data: any[] = [];
  data_landTDC_filter: NzSafeAny = [];
  data_BlockHouseTDC_filter: NzSafeAny = [];
  data_FloorTDC_filter: NzSafeAny = [];
  data_ApartmentTDC_filter: NzSafeAny = [];
  data_PlatformTDC_filter: NzSafeAny = [];

  block_value: boolean = false;
  floor_value: boolean = false;

  invalid_tableItemRef = true;

  check = false;

  tempTotalArea = 0;
  tempTotalPrice = 0;

  offiTotalArea = 0;
  offiTotalPrice = 0;

  moneyCenter = 0;

  offiMoneyPrincipal =0;
  offiManage2 = 0;
  offiManage3 =0;
  offiMintenance = 0;
  offiVAT = 0;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private TdcPriceOneSellRepository: TdcPriceOneSellRepository,
    private modalSrv: NzModalService,
    private tdcCustomerRepository: TdcCustomerRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository,
    private platformTdcRepository: PlatformTdcRepository,
  ) { }

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
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined],
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined],
      TdcApartmentId: [this.record ? this.record.TdcApartmentId : undefined],
      PlatformId: [this.record ? this.record.PlatformId : undefined],
      PersonalTax: [this.record ? this.record.PersonalTax : undefined, [Validators.required]],//thue thu nhap ca nhan
      RegistrationTax: [this.record ? this.record.RegistrationTax : undefined, [Validators.required]],//thue truoc ba
      tdcPriceOneSellTemporaries: [this.record ? this.record.tdcPriceOneSellTemporaries : []],
      tdcPriceOneSellOfficials: [this.record ? this.record.tdcPriceOneSellOfficials : []],
      tdcPriceOneSellTaxes: [this.record ? this.record.tdcPriceOneSellTaxes:[], []],
      Corner: [this.record ? this.record.Corner : false],
      DecisionNumberTT: [this.record ? this.record.DecisionNumberTT : undefined],
      DecisionDateTT: [this.record ? convertDate(this.record.DecisionDateTT) : undefined],
      DecisionNumberCT: [this.record ? this.record.DecisionNumberCT : undefined],
      DecisionDateCT: [this.record ? convertDate(this.record.DecisionDateCT) : undefined, [Validators.required]],
      TotalPriceTT: [{value: this.record ? this.record.TotalPriceTT : 0, disabled: true}],
      TotalPriceCT: [{value: this.record ? this.record.TotalPriceCT : 0, disabled: true }],
      TotalAreaTT: [{value: this.record ? this.record.TotalAreaTT : 0, disabled: true}],
      TotalAreaCT: [{value: this.record ? this.record.TotalAreaCT : 0, disabled: true}],
      MoneyPrincipalTT: [this.record ? this.record.MoneyPrincipalTT : undefined],
      Manage2TT: [this.record ? this.record.Manage2TT : undefined],
      Manage3TT: [this.record ? this.record.Manage3TT : undefined],
      MintenanceTT: [this.record ? this.record.MintenanceTT : undefined],
      VATTT: [this.record ? this.record.VATTT : undefined],
      MoneyPrincipalCT: [this.record ? this.record.MoneyPrincipalCT : undefined],
      Manage2CT: [this.record ? this.record.Manage2CT : undefined],
      Manage3CT: [this.record ? this.record.Manage3CT : undefined],
      MintenanceCT: [this.record ? this.record.MintenanceCT : undefined],
      VATCT: [this.record ? this.record.VATCT : undefined],
      PaymentPublic: [this.record ? this.record.PaymentPublic : undefined],
      
    });

    this.getDataCustomerTDC();
    this.getDataProjectTDC();

    if (this.record) {
      this.check = this.validateForm.value.Corner;
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataBlockHouseTDC(this.record.LandId);
      this.getDataFloorTDC(this.record.BlockHouseId);
      this.getApartmentTDC(this.record.FloorTdcId);
      this.getDataPlatformTDC(this.record.LandId);
    }

    if (this.validateForm.value.BlockHouseId) {
      this.block_value = true;
    } else {
      this.block_value = false;
    }
    if (this.validateForm.value.FloorTdcId) {
      this.floor_value = true;
    } else {
      this.floor_value = false;
    }
  }

  async submitForm() {
    this.loading = true;
    this.validateForm.get('tdcPriceOneSellTemporaries')?.setValue(this.TdcPriceOneSellTemporaryComponent.tableItemRef._data);
    this.validateForm.get('tdcPriceOneSellOfficials')?.setValue(this.TdcPriceOneSellOfficialComponent.tableItemRef._data);
    let data = { ...this.validateForm.getRawValue() };
    console.log(data);
    
    
    data.tdcPriceOneSellTaxes = this.TdcPriceOneSellTaxComponent.getValue();
    data.Corner = this.check;
    const resp = data.Id ? await this.TdcPriceOneSellRepository.update(data) : await this.TdcPriceOneSellRepository.addNew(data);
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
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  async getDataProjectTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  //gia ban cau thanh tam thoi
  SetIngrePriceTemp() {
    const newArray: { IngredientsPriceId: number; IngrePriceName: string; Area: number; Price: number; Total: number; 
      }[] =
      this.tdcProject_Filter_data.map(x => ({
        IngrePriceName: x.IngrePriceName,
        IngredientsPriceId: x.IngredientsPriceId,
        Area: 0,
        Price: 0,
        Total: 0,
         
      }));
    this.validateForm.get('tdcPriceOneSellTemporaries')?.setValue(newArray);
  }

  //gia ban cau thanh chinh thuc
  SetIngrePriceOffi(){
    const newArray: { IngredientsPriceId: number; IngrePriceName: string; Area: number; Price: number; Total: number; 
      }[] =
      this.tdcProject_Filter_data.map(x => ({
        IngrePriceName: x.IngrePriceName,
        IngredientsPriceId: x.IngredientsPriceId,
        Area: 0,
        Price: 0,
        Total: 0,
        
      }));
    this.validateForm.get('tdcPriceOneSellOfficials')?.setValue(newArray);
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

  //du lieu lo
  async getDataLandTDC(tdcProjectId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TdcProjectId=${tdcProjectId}`;
    paging.select = 'Id,Code,Name,TdcProjectId';

    const resp = await this.landRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcLand_data = resp.data;
      this.data_landTDC_filter = this.tdcLand_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  async TDCLandByProject(event: number) {
    if (event) {
      this.getDataLandTDC(event);
      this.SetIngrePrice(event);
    }
  }

  //du lieu khoi
  async getDataBlockHouseTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId';

    const resp = await this.blockHouseRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcBlockHouse_data = resp.data;
      this.data_BlockHouseTDC_filter = this.tdcBlockHouse_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  

  //du lieu nen dat
  async getDataPlatformTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId';

    const resp = await this.platformTdcRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcPlatform_data = resp.data;
      this.data_PlatformTDC_filter = this.tdcPlatform_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  TDCBlockHouseByLand(event: number) {
    if (event) {
      this.getDataBlockHouseTDC(event);
      this.getDataPlatformTDC(event);
    }
  }

  //du lieu tang
  async getDataFloorTDC(BlockHouseId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `BlockHouseId=${BlockHouseId}`;
    paging.select = 'Id,Code,Name,BlockHouseId,FloorNumber';

    const resp = await this.floorTdcRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcFloor_data = resp.data;
      this.data_FloorTDC_filter = this.tdcFloor_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  TDCFloorByBlockHouse(event: number) {
    if (event) {
      this.getDataFloorTDC(event);
      this.block_value = true;
    }
  }

  // du lieu can
  async getApartmentTDC(FloorTdcId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `FloorTdcId=${FloorTdcId}`;
    paging.select = 'Id,Code,Name,FloorTdcId,Corner';

    const resp = await this.apartmentTdcRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcApartment_data = resp.data;
      this.data_ApartmentTDC_filter = this.tdcApartment_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  TDCApartmentrByFloor(event: any) {
    if (event) {
      this.floor_value = true;
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
      let selectCorner = this.data_ApartmentTDC_filter.find((i: any) => i.Id === event);
      if (selectCorner) {
        if (selectCorner.Corner == true) this.check = true;
        else this.check = false;
      }
      let selectCorner2 = this.data_PlatformTDC_filter.find((i: any) => i.Id === event);
      if (selectCorner2) {
        if (selectCorner2.Corner == true) this.check = true;
        else this.check = false;
      }
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

  showLog() {
    this.modalSrv.create({
      nzTitle: `Lịch sử thay đổi thành phần giá bán cấu thành chính thức`,
      nzWidth: '75vw',
      nzContent: ShowLogOfficialComponent,
      nzComponentParams: {
        Id: this.validateForm.value.Id,
        type :2
      }
    });
  }
}
