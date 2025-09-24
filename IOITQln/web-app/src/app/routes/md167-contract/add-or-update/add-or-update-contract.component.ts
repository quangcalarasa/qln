import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { RentalPeriodContract167Enum, RentalPurposeContract167Enum, TypePriceContract167Enum, Contract167TypeEnum, TypeHouse167 } from 'src/app/shared/utils/enums';
import { TypePriceContract167, RentalPeriodContract167, RentalPurposeContract167, PaymentPeriodContract167, ContractStatus167 } from 'src/app/shared/utils/consts';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167DelegateRepository } from 'src/app/infrastructure/repositories/md167delegate.repository';
import { PricePerMonthComponent } from '../price-per-month/price-per-month.component';
import { ValuationComponent } from '../valuation/valuation.component';
import { AuctionDecisionComponent } from '../auction-decision/auction-decision.component';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AddOrUpdateMd167ContractExtraComponent } from '../add-or-update-extra/add-or-update-contract-extra.component';
import { Md167FileRepository } from 'src/app/infrastructure/repositories/reportwordmd167.repository';

@Component({
  selector: 'app-add-or-update-md167-contract',
  templateUrl: './add-or-update-contract.component.html'
})
export class AddOrUpdateMd167ContractComponent implements OnInit {
  @ViewChild('pricePerMonthComponent') pricePerMonthComponent!: PricePerMonthComponent;
  @ViewChild('valuationComponent') valuationComponent!: ValuationComponent;
  @ViewChild('auctionDecisionComponent') auctionDecisionComponent!: AuctionDecisionComponent;

  @ViewChild('ceTable') ceTable!: any;
  @ViewChild('liTable') liTable!: any;

  @Input() record: NzSafeAny;

  validateForm!: FormGroup;
  loading: boolean = false;
  curr_date: Date = new Date();

  type_price_data = TypePriceContract167;
  rental_period_data = RentalPeriodContract167;
  rental_purpose_data = RentalPurposeContract167;
  payment_period_data = PaymentPeriodContract167;
  contract_status_data = ContractStatus167;

  delegaties: any[] = [];
  house_data: any[] = [];
  pdw_data: any[] = [];
  idPerson=0;
  idCom=0;
  lane_data: any[] = [];
  RentalPeriodContract167Enum = RentalPeriodContract167Enum;
  RentalPurposeContract167Enum = RentalPurposeContract167Enum;

  delegate: any;
  house: any;
  isPerson=true;

  invalid_price_per_month = false;
  invalid_valuation = false;
  invalid_auction_decision = false;

  TypeHouse167 = TypeHouse167;
  nzFormat = 'dd/ MM/ yyyy';
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private md167DelegateRepository: Md167DelegateRepository,
    private md167ContractRepository: Md167ContractRepository,
    private md167HouseRepository: Md167HouseRepository,
    private md167FileRepository: Md167FileRepository,
    private provinceRepository: ProvinceRepository,
    private laneRepository: LaneRepository,
    private modalSrv: NzModalService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    if(this.record) {
      this.isPerson=this.record.personOrCompany
      this.record.personOrCompany?this.idPerson=this.record.DelegateId:this.idCom=this.record.DelegateId
    }
    
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],    //Mã hợp đồng
      // ProfileCode: [this.record ? this.record.ProfileCode : undefined, [Validators.required]],    //Mã số hồ sơ
      DateSign: [this.record ? convertDate(this.record.DateSign) : convertDate(this.curr_date.toDateString()), [Validators.required]],   //Ngày ký
      DelegateId: [this.record ? this.record.DelegateId : undefined, [Validators.required]],    //người thuê
      HouseId: [this.record ? this.record.HouseId : undefined, [Validators.required]],    //Căn nhà
      personOrCompany: [this.record ? this.record.personOrCompany : true ],    //Căn nhà

      TypePrice: [this.record ? this.record.TypePrice.toString() : undefined, [Validators.required]],    //Loại giá: Giá niêm yết/đấu giá
      RentalPeriod: [this.record ? this.record.RentalPeriod.toString() : undefined, [Validators.required]],    //Thời gian thuê
      NoteRentalPeriod: [this.record ? this.record.NoteRentalPeriod : undefined, []],    //Ghi chú thêm về thời gian thuê
      RentalPurpose: [this.record ? this.record.RentalPurpose.toString() : undefined, [Validators.required]],    //Mục đích gian thuê
      NoteRentalPurpose: [this.record ? this.record.NoteRentalPurpose : undefined, []],    //Ghi chú thêm về mục đích thuê
      PaymentPeriod: [this.record ? this.record.PaymentPeriod.toString() : undefined, [Validators.required]],    //Kỳ thanh toán
      DateGroundHandover: [this.record ? convertDate(this.record.DateGroundHandover) : undefined, [Validators.required]],   //Ngày bàn giao mặt bằng
      ContractStatus: [this.record ? this.record.ContractStatus.toString() : undefined, [Validators.required]],    //Trạng thái hợp đồng
      ParentId: [this.record ? this.record.ParentId : undefined, []],    //Id Hợp đồng cha
      Type: [this.record ? this.record.Type : Contract167TypeEnum.MAIN, [Validators.required]],    //Loại hợp đồng: hợp đồng hoặc phụ lục
      pricePerMonths: [this.record ? this.record.pricePerMonths.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : [], []],    //Danh sách Số tiền/tháng
      valuations: [this.record ? this.record.valuations.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : [], []],    //Danh sách Thẩm định giá
      auctionDecisions: [this.record ? this.record.auctionDecisions.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : [], []],    //Danh sách Quyết định đấu giá
      contractExtensions: [this.record ? this.record.contractExtensions : [], []],    //Danh sách Ngày gia hạn hợp đồng
      liquidations: [this.record ? this.record.liquidations : [], []]    //Danh sách Ngày thanh lý hợp đồng
    });

    this.getDelegateData();
    this.getHouseData();
    this.getCascaderData();
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    data.contractExtensions = data.contractExtensions.filter((x: any) => x != undefined && x != '');
    data.liquidations = data.liquidations.filter((x: any) => x != undefined && x != '');

    data.pricePerMonths = this.pricePerMonthComponent.getValue();
    data.valuations = this.valuationComponent.getValue();
    data.auctionDecisions = this.auctionDecisionComponent.getValue();

    const resp = data.Id ? await this.md167ContractRepository.update(data) : await this.md167ContractRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }
  changePerSon()
  {
    if(this.isPerson)
    {
      this.idPerson=this.validateForm.value.DelegateId
      this.validateForm.get('DelegateId')?.setValue(this.idCom);
    }
    else{
      this.idCom=this.validateForm.value.DelegateId
      this.validateForm.get('DelegateId')?.setValue(this.idPerson);
    }
    this.isPerson=!this.isPerson;
    this.validateForm.get('personOrCompany')?.setValue(!this.validateForm.value.personOrCompany);
    this.getDelegateData()
  }
  close(): void {
    this.drawerRef.close();
  }

  addRow(cs: number) {
    if (cs == 1) {
      this.validateForm.value.contractExtensions.unshift(undefined);
      this.ceTable.data.unshift(undefined);
    }
    else {
      this.validateForm.value.liquidations.unshift(undefined);
      this.liTable.data.unshift(undefined);
    }
  }

  removeRow(index: number, cs: number) {
    if (cs == 1) {
      this.validateForm.value.contractExtensions.splice(index, 1);
      this.ceTable.data.splice(index, 1);
    }
    else {
      this.validateForm.value.liquidations.splice(index, 1);
      this.liTable.data.splice(index, 1);
    }
  }

  async getDelegateData() {
    let paging: GetByPageModel = new GetByPageModel();
    if(this.isPerson==true)  paging.query = `PersonOrCompany=1`;
    else paging.query = `PersonOrCompany=2`;
    paging.page_size = 0;
    paging.select = "Id, Name, NationalId, PhoneNumber, Address, ComTaxNumber";

    const resp = await this.md167DelegateRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.delegaties = resp.data;
      if (this.validateForm.value.Id) {
        this.selectDelegate(this.validateForm.value.DelegateId);
      }
    }
  }

  selectDelegate(id: number) {
    this.delegate = undefined;
    if (id) {
      this.delegate = this.delegaties.find(x => x.Id == id);
    }
  }

  async getHouseData() {
    const resp = await this.md167ContractRepository.GetHouseData();

    if (resp.meta?.error_code == 200) {
      this.house_data = resp.data;
      if (this.validateForm.value.Id) {
        this.selectHouse(this.validateForm.value.HouseId);
      }
    }
  }

  selectHouse(id: number) {
    this.house = undefined;
    if (id) {
      this.house = this.house_data.find(x => x.Id == id);
      this.house.Pdw = [this.house.ProvinceId, this.house.DistrictId, this.house.WardId];
      this.getLaneData(this.house.WardId);
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
  async getCascaderData() {
    const resp = await this.provinceRepository.getCascaderData(1);

    if (resp.meta?.error_code == 200) {
      this.pdw_data = resp.data;
    }
  }

  //danh sách đường theo WardId
  async getLaneData(wardId?: number) {
    this.lane_data = [];
    if (!wardId) return;

    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Ward=${wardId}`;

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_data = resp.data;
    }
  }

  selectRentalPeriod(id: number) {
    if (id) {
      if (id == RentalPeriodContract167Enum.THUE_5_NAM) {
        this.validateForm.get("TypePrice")?.setValue(TypePriceContract167Enum.DAU_GIA.toString());
      }
      else {
        this.validateForm.get("TypePrice")?.setValue(TypePriceContract167Enum.GIA_NIEM_YET.toString());
      }
    }
    else {
      this.validateForm.get("TypePrice")?.setValue(undefined);
    }
  }

  //Thêm phụ lục hợp đồng
  addContractExtra() {
    this.modalSrv.create({
      nzTitle: `Phụ lục cho hợp đồng "${this.validateForm.value.Code}"`,
      nzContent: AddOrUpdateMd167ContractExtraComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record: undefined,
        parent: this.validateForm.value
      },
      nzOnOk: (res: any) => {
        this.drawerRef.close(true);
        this.message.create('success', `Thêm phụ lục cho hợp đồng thành công!`);
      }
    });
  }

  onClick(){
    this.md167FileRepository.GetExportContract1(this.record.Id);
  }
  onClick1(){
    this.md167FileRepository.GetExportContract3(this.record.Id);
  }

  onClick2(){
    this.md167FileRepository.GetExportContract4(this.record.Id);
  }
}
