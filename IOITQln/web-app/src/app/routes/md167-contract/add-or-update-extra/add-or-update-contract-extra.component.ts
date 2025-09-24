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
import { NzModalRef } from 'ng-zorro-antd/modal';
import { Md167FileRepository } from 'src/app/infrastructure/repositories/reportwordmd167.repository';

@Component({
  selector: 'app-add-or-update-md167-contract-extra',
  templateUrl: './add-or-update-contract-extra.component.html'
})
export class AddOrUpdateMd167ContractExtraComponent implements OnInit {
  @ViewChild('pricePerMonthComponent') pricePerMonthComponent!: PricePerMonthComponent;
  @ViewChild('valuationComponent') valuationComponent!: ValuationComponent;
  @ViewChild('auctionDecisionComponent') auctionDecisionComponent!: AuctionDecisionComponent;

  @Input() record: any;
  @Input() parent: any;
  nzFormat = 'dd/ MM/ yyyy';
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
  lane_data: any[] = [];

  RentalPeriodContract167Enum = RentalPeriodContract167Enum;
  RentalPurposeContract167Enum = RentalPurposeContract167Enum;

  delegate: any;
  house: any;

  invalid_price_per_month = false;
  invalid_valuation = false;
  invalid_auction_decision = false;

  TypeHouse167 = TypeHouse167;

  constructor(
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private md167DelegateRepository: Md167DelegateRepository,
    private md167ContractRepository: Md167ContractRepository,
    private md167HouseRepository: Md167HouseRepository,
    private md167FileRepository: Md167FileRepository,
    private provinceRepository: ProvinceRepository,
    private laneRepository: LaneRepository,
    private modal: NzModalRef
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],    //Mã hợp đồng
      // ProfileCode: [this.record ? this.record.ProfileCode : this.parent.ProfileCode, [Validators.required]],    //Mã số hồ sơ
      DateSign: [this.record ? convertDate(this.record.DateSign) : convertDate(this.curr_date.toDateString()), [Validators.required]],   //Ngày ký
      DelegateId: [this.record ? this.record.DelegateId : this.parent.DelegateId, [Validators.required]],    //người thuê
      HouseId: [this.record ? this.record.HouseId : this.parent.HouseId, [Validators.required]],    //Căn nhà
      TypePrice: [this.record ? this.record.TypePrice.toString() : this.parent.TypePrice.toString(), [Validators.required]],    //Loại giá: Giá niêm yết/đấu giá
      RentalPeriod: [this.record ? this.record.RentalPeriod.toString() : this.parent.RentalPeriod.toString(), [Validators.required]],    //Thời gian thuê
      NoteRentalPeriod: [this.record ? this.record.NoteRentalPeriod : this.parent.NoteRentalPeriod, []],    //Ghi chú thêm về thời gian thuê
      RentalPurpose: [this.record ? this.record.RentalPurpose.toString() : this.parent.RentalPurpose.toString(), [Validators.required]],    //Mục đích gian thuê
      NoteRentalPurpose: [this.record ? this.record.NoteRentalPurpose : this.parent.NoteRentalPurpose, []],    //Ghi chú thêm về mục đích thuê
      PaymentPeriod: [this.record ? this.record.PaymentPeriod.toString() : this.parent.PaymentPeriod.toString(), [Validators.required]],    //Kỳ thanh toán
      DateGroundHandover: [this.record ? convertDate(this.record.DateGroundHandover) : convertDate(this.parent.DateGroundHandover), [Validators.required]],   //Ngày bàn giao mặt bằng
      ContractStatus: [this.record ? this.record.ContractStatus.toString() : "1", [Validators.required]],    //Trạng thái hợp đồng
      ParentId: [this.record ? this.record.ParentId : this.parent.Id, []],    //Id Hợp đồng cha
      Type: [this.record ? this.record.Type : Contract167TypeEnum.EXTRA, [Validators.required]],    //Loại hợp đồng: hợp đồng hoặc phụ lục
      pricePerMonths: [this.record ? this.record.pricePerMonths.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : this.parent.pricePerMonths.map((item: any, index: number) => {
        delete item['Id'];
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }), []],    //Danh sách Số tiền/tháng
      valuations: [this.record ? this.record.valuations.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : this.parent.valuations.map((item: any, index: number) => {
        delete item['Id'];
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }), []],    //Danh sách Thẩm định giá
      auctionDecisions: [this.record ? this.record.auctionDecisions.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }) : this.parent.auctionDecisions.map((item: any, index: number) => {
        delete item['Id'];
        item.index = index + 1;
        item.DateEffect = convertDate(item.DateEffect);
        return item;
      }), []],    //Danh sách Quyết định đấu giá
      contractExtensions: [this.record ? this.record.contractExtensions : this.parent.contractExtensions, []],    //Danh sách Ngày gia hạn hợp đồng
      liquidations: [this.record ? this.record.liquidations : this.parent.liquidations, []]    //Danh sách Ngày thanh lý hợp đồng
    });

    this.getDelegateData();
    this.getHouseData();
    this.getCascaderData();
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
      this.modal.triggerOk();
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.modal.close();
  }

  addRow(cs: number) {
    if (cs == 1) {
      this.validateForm.value.contractExtensions.unshift(undefined);
    }
    else {
      this.validateForm.value.liquidations.unshift(undefined);
    }
  }

  removeRow(index: number, cs: number) {
    if (cs == 1) {
      this.validateForm.value.contractExtensions.splice(index, 1);
    }
    else {
      this.validateForm.value.liquidations.splice(index, 1);
    }
  }

  async getDelegateData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = "Id, Name, NationalId, PhoneNumber, Address";

    const resp = await this.md167DelegateRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.delegaties = resp.data;
      this.selectDelegate(this.validateForm.value.DelegateId);
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
      this.selectHouse(this.validateForm.value.HouseId);
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

  onClick(){
    this.md167FileRepository.GetExportContract2(this.record.Id);
  }
  onClick1(){
    this.md167FileRepository.GetExportContract4(this.record.Id);
  }
  //Thêm phụ lục hợp đồng
}
