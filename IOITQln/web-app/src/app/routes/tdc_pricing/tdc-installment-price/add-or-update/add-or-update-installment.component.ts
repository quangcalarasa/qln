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

import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { PlatformTdcRepository } from 'src/app/infrastructure/repositories/platform-tdc.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { TdcCustomerRepository } from 'src/app/infrastructure/repositories/tdcCustomer.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { TdcInstallmentTemporaryComponent } from '../tdc-installment-temporary/tdc-installment-temporary.component';
import { TdcInstallmentPriceAndTaxComponent } from '../tdc-installment-price-and-tax/tdc-installment-price-and-tax.component';
import { TdcInstallmentOfficalComponent } from '../tdc-installment-offical/tdc-installment-offical.component';
import { AddendumTDCComponent } from '../addendum-tdc/addendum-tdc.component';
import { ShowLogTemporaryComponent } from '../../tdc-price-rent/show-log-temporary/show-log-temporary.component';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update-installment.component.html',
  styles: []
})
export class AddOrUpdateInstallmentComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @ViewChild('tdcInstallmentTemporaryComponent') private tdcInstallmentTemporaryComponent!: TdcInstallmentTemporaryComponent;
  @ViewChild('tDCPricAndTaxComponent') private tDCPricAndTaxComponent!: TdcInstallmentPriceAndTaxComponent;
  @ViewChild('tdcInstallmentOfficalComponent') private tdcInstallmentOfficalComponent!: TdcInstallmentOfficalComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;

  check = false;
  tdcCustomer_data: any[] = [];
  tdcProject_data: any[] = [];
  ingre_data: any[] = [];
  tdcLand_data = [];
  tdcBlockHouse_data = [];
  tdcPlatform_data = [];
  tdcFloor_data = [];
  tdcApartment_data = [];
  temporaryTotalArea = 0;
  temporaryTotalPrice = 0;
  TotalArea = 0;
  TotalPrice = 0;

  data_landTDC_filter: NzSafeAny = [];
  data_BlockHouseTDC_filter: NzSafeAny = [];
  data_FloorTDC_filter: NzSafeAny = [];
  data_ApartmentTDC_filter: NzSafeAny = [];
  data_PlatformTdc_fliter: NzSafeAny = [];

  block_value: boolean = false;
  floor_value: boolean = false;

  change = false;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
    private modalSrv: NzModalService,
    private tdcCustomerRepository: TdcCustomerRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository,
    private platformTdcRepository: PlatformTdcRepository
  ) {}

  ngOnInit(): void {
    if (localStorage.getItem('add') == 'false') this.add = false;
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      ContractNumber: [this.record ? this.record.ContractNumber : undefined, [Validators.required]], //số hợp đồng
      DateNumber: [this.record ? convertDate(this.record.DateNumber) : undefined, [Validators.required]], //ngày hợp đồng
      TdcCustomerId: [this.record ? this.record.TdcCustomerId : undefined, [Validators.required]], //id khách hàng
      DifferenceValue: [{ value: this.record ? this.record.DifferenceValue : 0, disabled: true }], // giá trị chênh lệch= NewContractValue - OldContractValue
      //Qfix
      //NewContractValue: [{ value: this.record ? this.record.NewContractValue : 0, disabled: true }], //giá trị hợp đồng mới
      //OldContractValue: [{ value: this.record ? this.record.OldContractValue : 0, disabled: true }], //giá trị hợp đồng cũ
      NewContractValue: [{ value: this.record ? this.record.NewContractValue : 0, disabled: false }], //giá trị hợp đồng mới
      OldContractValue: [{ value: this.record ? this.record.OldContractValue : 0, disabled: false }], //giá trị hợp đồng cũ
      Floor1: [this.record ? this.record.Floor1 : undefined], //id lầu
      TdcProjectId: [this.record ? this.record.TdcProjectId : undefined, [Validators.required]], //id dự án
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined],
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined],
      TdcApartmentId: [this.record ? this.record.TdcApartmentId : undefined],
      PlatformId: [this.record ? this.record.PlatformId : undefined],
      YearPay: [this.record ? this.record.YearPay : undefined, [Validators.required]], //số năm trả
      //Qfix
      //TemporaryDecreeNumber: [this.record ? this.record.TemporaryDecreeNumber : undefined, [Validators.required]], //số quyết định tạm thời
      TemporaryDecreeNumber: [this.record ? this.record.TemporaryDecreeNumber : undefined], //số quyết định tạm thời
      //TemporaryDecreeDate: [this.record ? convertDate(this.record.TemporaryDecreeDate) : undefined, [Validators.required]], //ngày quyết định tạm thời
      TemporaryDecreeDate: [this.record ? convertDate(this.record.TemporaryDecreeDate) : undefined], //ngày quyết định tạm thời
      TemporaryTotalArea: [{ value: this.record ? this.record.TemporaryTotalArea : 0, disabled: true }], //Diện tích tạm thời
      TemporaryTotalPrice: [{ value: this.record ? this.record.TemporaryTotalPrice : 0, disabled: true }], //Thành tiền tạm thời
      DecreeNumber: [this.record ? this.record.DecreeNumber : undefined], //số quyết định chính thức
      DecreeDate: [this.record ? convertDate(this.record.DecreeDate) : undefined], //ngày quyết định chính thức
      TotalArea: [{ value: this.record ? this.record.TotalArea : 0, disabled: true }], //Diện tích chính thức
      TotalPrice: [{ value: this.record ? this.record.TotalPrice : 0, disabled: true }], //Thành tiền chính thức
      FirstPay: [this.record ? this.record.FirstPay : undefined, [Validators.required]], // số tiền trả lần đầu
      FirstPayDate: [this.record ? convertDate(this.record.FirstPayDate) : undefined], // ngày tiền trả lần đầu
      TotalPayValue: [{ value: this.record ? this.record.TotalPayValue : 0, disabled: true }], // tổng số tiền phải trả
      tDCInstallmentTemporaryDetails: [this.record ? this.record.tDCInstallmentTemporaryDetails : []],
      tDCInstallmentPriceAndTaxs: [this.record ? this.record.tDCInstallmentPriceAndTaxs : []],
      tDCInstallmentOfficialDetails: [this.record ? this.record.tDCInstallmentOfficialDetails : []],
      DateTDC: [this.record ? convertDate(this.record.DateTDC) : undefined, [Validators.required]],
      Corner: [this.record ? this.record.Corner : false],
      PesonalTax: [this.record ? this.record.PesonalTax : undefined, [Validators.required]],
      RegistrationTax: [this.record ? this.record.RegistrationTax : undefined, [Validators.required]]
    });

    this.getDataCustomerTDC();
    this.getDataProjectTDC();

    if (this.record) {
      // this.setIngrePrice(this.record.TdcProjectId);
      this.temporaryTotalArea = this.record.TemporaryTotalArea;
      this.temporaryTotalPrice = this.record.TemporaryTotalPrice;
      this.TotalArea = this.record.TotalArea;
      this.TotalPrice = this.record.TotalPrice;
      this.check = this.validateForm.value.Corner;
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataBlockHouseTDC(this.record.LandId);
      this.getDataFloorTDC(this.record.BlockHouseId);
      this.getApartmentTDC(this.record.FloorTdcId);
      this.getPlatformTDC(this.record.LandId);
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

  changeDataTemporaryArea(event: any) {
    let area = 0;
    let price = 0;
    for (let i = 0; i < event.length; i++) {
      area += event[i].Area;
      price += event[i].Price;
    }
    this.temporaryTotalArea = area;
    this.temporaryTotalPrice = price;
  }
  showLog() {
    this.modalSrv.create({
      nzTitle: `Lịch sử thay đổi thành phần giá bán cấu thành chính thức`,
      nzWidth: '75vw',
      nzContent: ShowLogTemporaryComponent,
      nzComponentParams: {
        Id: this.validateForm.value.Id,
        type:3
      }
    });
  }
  changeDataOfficalArea(event: any) {
    let area = 0;
    let price = 0;
    for (let i = 0; i < event.length; i++) {
      area += event[i].Area;
      price += event[i].Price;
    }
    this.TotalArea = area;
    this.TotalPrice = price;
  }
  calcTotalArea() {
    return this.TotalArea;
  }
  calcTotalPrice() {
    return this.TotalPrice;
  }

  calcTemporaryTotalArea() {
    return this.temporaryTotalArea;
  }

  calcTemporaryTotalPrice() {
    return this.temporaryTotalPrice;
  }

  calcTotalPayValue() {
    if (this.validateForm.value.DecreeNumber !== null && this.validateForm.value.DecreeDate !== '1-01-01') {
      this.validateForm.get('TotalPayValue')?.setValue(this.TotalPrice - this.validateForm.value.FirstPay);
      return this.TotalPrice - this.validateForm.value.FirstPay;
    }
    this.validateForm.get('TotalPayValue')?.setValue(this.temporaryTotalPrice - this.validateForm.value.FirstPay);
    return this.temporaryTotalPrice - this.validateForm.value.FirstPay;
  }

  async submitForm() {
    this.loading = true;

    this.validateForm.get('tDCInstallmentTemporaryDetails')?.setValue(this.tdcInstallmentTemporaryComponent.tableItemRef._data);
    this.validateForm.get('tDCInstallmentOfficialDetails')?.setValue(this.tdcInstallmentOfficalComponent.tableItemRef._data);

    let data = { ...this.validateForm.value };
    console.log(this.temporaryTotalPrice);
    data.OldContractValue = data.TemporaryTotalPrice = this.temporaryTotalPrice;
    data.TemporaryTotalArea = this.temporaryTotalArea;
    data.NewContractValue = data.TotalPrice = this.TotalPrice;
    data.TotalArea = this.TotalArea;
    data.DifferenceValue = data.NewContractValue - data.OldContractValue;
    data.tDCInstallmentPriceAndTaxs = this.tDCPricAndTaxComponent.tableItemRef._data;
    data.DifferenceValue = data.NewContractValue - data.OldContractValue;
    data.TotalPayValue = this.TotalPrice - this.validateForm.value.FirstPay;
    if (this.validateForm.value.DecreeNumber !== null && this.validateForm.value.DecreeDate !== '1-01-01') {
      data.TotalPayValue = this.TotalPrice - this.validateForm.value.FirstPay;
    } else data.TotalPayValue = this.temporaryTotalPrice - this.validateForm.value.FirstPay;
    const resp = data.Id ? await this.tDCInstallmentPriceRepository.update(data) : await this.tDCInstallmentPriceRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  TdcCornerByApartment(event: any) {
    if (event) {
      let selectCorner = this.data_ApartmentTDC_filter.find((i: any) => i.Id === event);
      if (selectCorner) {
        if (selectCorner.Corner == true) this.check = true;
        else this.check = false;
      }
      let selectCorner2 = this.data_PlatformTdc_fliter.find((i: any) => i.Id === event);
      if (selectCorner2) {
        if (selectCorner2.Corner == true) this.check = true;
        else this.check = false;
      }
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
    paging.select = 'Id,Code,Name';

    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
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
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  TDCLandByProject(event: number) {
    if (event) {
      this.getDataLandTDC(event);
      this.getIngrePrice(event);
    }
  }

  async getPlatformTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,Corner';

    const resp = await this.platformTdcRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcPlatform_data = resp.data;
      this.data_PlatformTdc_fliter = this.tdcPlatform_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  async getIngrePrice(event: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    paging.select = '';
    const resp = await this.tdcProjectRepository.getByPage(paging);
    this.ingre_data = await resp.data[0].tDCProjectIngrePrices;
    this.setTemporary();
  }

  setTemporary() {
    const newArray: { IngredientsPriceId: number; IngrePriceName: string; Area: number; UnitPrice: number; Price: number }[] =
      this.ingre_data.map(x => ({
        IngrePriceName: x.IngrePriceName,
        IngredientsPriceId: x.IngredientsPriceId,
        Area: 0,
        UnitPrice: 0,
        Price: 0
      }));
    // this.validateForm.value.tDCInstallmentTemporaryDetails = newArray;
    this.validateForm.get('tDCInstallmentTemporaryDetails')?.setValue(newArray);
    this.validateForm.get('tDCInstallmentOfficialDetails')?.setValue(newArray);
  }

  ///Lấy Dữ Liệu Khối
  async getDataBlockHouseTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId';

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

  TDCBlockHouseByLand(event: number) {
    if (event) {
      this.getDataBlockHouseTDC(event);
      this.getPlatformTDC(event);
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
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  calcDifferenceValue() {
    if (this.validateForm.value.DecreeNumber !== null && this.validateForm.value.DecreeDate !== '1-01-01') {
      this.validateForm.get('DifferenceValue')?.setValue(this.TotalPrice - this.temporaryTotalPrice);
      return this.validateForm.value.DifferenceValue;
    }
    this.validateForm.get('DifferenceValue')?.setValue(0);
    return 0;
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

  //Thêm phụ lục hợp đồng
  addContractExtra() {
    this.modalSrv.create({
      nzTitle: `Phụ lục cho hợp đồng "${this.validateForm.value.ContractNumber}"`,
      nzContent: AddendumTDCComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record: undefined,
        parent: this.validateForm.value
      },
      nzOnOk: (res: any) => {
        this.drawerRef.close(true);
        this.message.create('success', `Thêm phụ lục cho hợp đồng thành công!`);
        this.change = !this.change;
      }
    });
  }
}
