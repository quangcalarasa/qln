import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PricingRepository } from 'src/app/infrastructure/repositories/pricing.repository';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import { PriceListRepository } from 'src/app/infrastructure/repositories/price-list.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { LocationResidentialLand, LevelBlock } from 'src/app/shared/utils/consts';

import { LandPriceRepository } from 'src/app/infrastructure/repositories/land-price.repository';
import { PositionCoefficientRepository } from 'src/app/infrastructure/repositories/position-coefficient.repository';
import { SalaryCoefficientRepository } from 'src/app/infrastructure/repositories/salary-coefficient.repository';
import { ConstructionPriceRepository } from 'src/app/infrastructure/repositories/construction-price.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { PricingOfficerComponent } from '../pricing-officer/pricing-officer.component';
import { TypeReportApplyEnum, LandPriceType, AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-pricing',
  templateUrl: './add-or-update-pricing.component.html'
})
export class AddOrUpdatePricingComponent implements OnInit {
  @ViewChild('pricingOfficerComponent') private pricingOfficerComponent!: PricingOfficerComponent;

  validateForm!: FormGroup;
  loading: boolean = false;
  curr_date: Date = new Date();

  @Input() init_pricing: NzSafeAny;
  @Input() record: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;
  @Input() vat_data: NzSafeAny;
  @Input() customer_data: any[] = [];
  @Input() deduction_coefficient_data: any[] = [];
  @Input() editHistory: NzSafeAny;
  @Input() isViewRecord?: boolean;
  
  block: any;
  apartment: any;

  constructionPriceItem_data: any[] = [];
  price_list_item_data: any[] = [];
  landprice_data: any[] = [];
  position_coefficient_data: any[] = [];
  constructionPricies_data: any[] = [];

  salary_default?: number = undefined;
  invalidTblPricingOfficer = false;
  invalidTblReducedPerson = false;
  role = this.commonService.CheckAccessKeyRole(AccessKey.PRICING);
  TypeReportApplyEnum = TypeReportApplyEnum;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private blockRepository: BlockRepository,
    private apartmentRepository: ApartmentRepository,

    private landPriceRepository: LandPriceRepository,
    private positionCoefficientRepository: PositionCoefficientRepository,

    private salaryCoefficientRepository: SalaryCoefficientRepository,
    private constructionPriceRepository: ConstructionPriceRepository,
    private pricingRepository: PricingRepository,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private priceListRepository: PriceListRepository,
    private commonService: CommonService,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      TypeReportApply: [this.record ? this.record.TypeReportApply : this.init_pricing.TypeReportApply, [Validators.required]],
      BlockId: [this.record ? this.record.BlockId : this.init_pricing.BlockId, [Validators.required]],
      ApartmentId: [this.record ? this.record.ApartmentId : this.init_pricing.ApartmentId, []],
      DateCreate: [this.record ? convertDate(this.record.DateCreate) : convertDate(this.curr_date.toDateString()), []],
      TimeUse: [this.record ? this.record.TimeUse : undefined, []],
      pricingReplaceds: [this.record ? this.record.pricingReplaceds : (this.init_pricing.pricingReplaceds ? this.init_pricing.pricingReplaceds.map((item: any) => {
        item.PricingReplacedId = item.Id;
        delete item.Id;
        return item;
      }) : []), []], //Biên bản bị thay thế
      VatId: [this.record ? this.record.VatId : undefined, []],
      Vat: [this.record ? this.record.Vat : undefined, []],
      ApartmentPrice: [this.record ? this.record.ApartmentPrice : undefined, []], //Tổng tiền nhà
      ApartmentPriceReduced: [this.record ? this.record.ApartmentPriceReduced : undefined, []], //Tiền nhà được giảm
      ApartmentPriceRemaining: [this.record ? this.record.ApartmentPriceRemaining : undefined, [Validators.required]], //Tiền nhà còn lại
      ApartmentPriceNoVat: [this.record ? this.record.ApartmentPriceNoVat : undefined, []], //Tiền nhà chưa VAT
      ApartmentPriceVat: [this.record ? this.record.ApartmentPriceVat : undefined, []], //Tiền VAT
      ApartmentPriceReducedNote: [this.record ? this.record.ApartmentPriceReducedNote : undefined, []], //Ghi chú tiền nhà được giảm
      LandPrice: [this.record ? this.record.LandPrice : undefined, [Validators.required]], //Tổng tiền đất
      DeductionLandMoneyId: [this.record ? this.record.DeductionLandMoneyId : undefined, []],
      DeductionLandMoneyValue: [this.record ? this.record.DeductionLandMoneyValue : undefined, []],
      ConversionArea: [this.record ? this.record.ConversionArea : undefined, []], //Diện tích qui đổi
      LandPriceAfterReduced: [this.record ? this.record.LandPriceAfterReduced : undefined, []], //Giá đất sau khi được miễn giảm
      TotalPrice: [this.record ? this.record.TotalPrice : undefined, [Validators.required]], //Tổng giá bán căn nhà = Tiền đất + tiền nhà
      customers: [this.record ? (this.record.customers.length == 0 ? undefined : this.record.customers) : undefined, []], //Người thuê nhà
      constructionPricies: [
        this.record
          ? this.record.constructionPricies.length == 0
            ? undefined
            : this.record.constructionPricies
          : this.constructionPricies_data,
        []
      ], //Chỉ số giá xây dựng công trình
      landPricingTbl: [this.record ? (this.record.landPricingTbl.length == 0 ? [] : this.record.landPricingTbl) : [], []], //Tính giá phần nhà
      pricingOfficers: [
        this.record
          ? this.record.pricingOfficers.map((item: any, index: number) => {
            item.index = index + 1;
            return item;
          })
          : [],
        []
      ], //Cán bộ tính giá nhà
      reducedPerson: [this.record ? this.record.reducedPerson : [], []], //Người được tinh giảm
      pricingApartmentLandDetails: [this.record ? this.record.pricingApartmentLandDetails : [], []], //Thông tin tính giá đất,
      AreaCorrectionCoefficientId: [this.record ? this.record.AreaCorrectionCoefficientId : undefined, []], //Id Hệ số điều chỉnh vùng
      AreaCorrectionCoefficientValue: [this.record ? this.record.AreaCorrectionCoefficientValue : undefined, []], //Giá trị Hệ số điều chỉnh vùng
      FlatCoefficientId_99: [this.record ? this.record.FlatCoefficientId_99 : undefined, []],
      FlatCoefficient_99: [this.record ? this.record.FlatCoefficient_99 : undefined, []],
      FlatCoefficientId_34: [this.record ? this.record.FlatCoefficientId_34 : undefined, []],
      FlatCoefficient_34: [this.record ? this.record.FlatCoefficient_34 : undefined, []],
      FlatCoefficientId_61: [this.record ? this.record.FlatCoefficientId_61 : undefined, []],
      FlatCoefficient_61: [this.record ? this.record.FlatCoefficient_61 : undefined, []],
      SellLandArea: [this.record ? this.record.SellLandArea : undefined, []],                  //Diện tích đất có nhà ở đã bán
      ProcessProfileCeCode: [this.record ? this.record.ProcessProfileCeCode : undefined, []],
      CreatedAt: [this.record ? this.record.CreatedAt : undefined, []],
      UpdatedAt: [this.record ? this.record.UpdatedAt : undefined, []],
      CreatedBy: [this.record ? this.record.CreatedBy : undefined, []],
      UpdatedBy: [this.record ? this.record.UpdatedBy : undefined, []]
    });

    if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
      this.validateForm.get('ApartmentPriceRemaining')?.setValidators(null);
    }
    else {
      this.validateForm.get('ApartmentPriceRemaining')?.setValidators([Validators.required]);
    }

    this.validateForm.get('ApartmentPriceRemaining')?.updateValueAndValidity();

    this.getBlockById();
    this.getApartmentById();
    // this.genConstructionPriceItem();
    this.getPriceListItems();
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    if (data.customers) {
      data.customers.forEach((x: any) => {
        x.CustomerId = x.CustomerId ?? x.Id;

        return x;
      });
    }

    if (data.constructionPricies) {
      data.constructionPricies.forEach((x: any) => {
        x.ConstructionPriceId = x.ConstructionPriceId ?? x.Id;

        return x;
      });
    }

    data.pricingOfficers = this.pricingOfficerComponent.tableItemRef._data;

    if (data.pricingReplaceds) {
      data.pricingReplaceds.forEach((x: any) => {
        x.PricingReplacedId = x.PricingReplacedId ?? x.Id;

        delete x["Id"];
        return x;
      });
    }

    data.editHistory = this.editHistory;

    const resp = data.Id ? await this.pricingRepository.update(data) : await this.pricingRepository.addNew(data);
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

  async getBlockById() {
    let blockId = this.validateForm.value.BlockId;

    const resp = await this.blockRepository.getById(blockId);

    if (resp.meta?.error_code == 200) {
      this.block = resp.data;
      this.block.LandscapeLocation = this.block.LandscapeLocation ? this.block.LandscapeLocation.toString() : undefined;
      // this.getDeductionLandMoneyData(this.block.DecreeType1Id);
      this.getLandPrice();
      // this.getPositionCoefficient(this.block.DecreeType1Id, this.block.LandscapeLocation, this.block.PositionCoefficientId);
      this.getByDecreeAndDate();
      this.getConstructionPriceData();
    } else {
      this.message.create('error', 'Không tìm thấy căn nhà!');
      this.close();
    }
  }

  async getApartmentById() {
    let apartmentId = this.validateForm.value.ApartmentId;

    if (apartmentId) {
      const resp = await this.apartmentRepository.getById(apartmentId);

      if (resp.meta?.error_code == 200) {
        this.apartment = resp.data;
      } else {
        this.message.create('error', 'Không tìm thấy căn hộ!');
        this.close();
      }
    }
  }

  // genConstructionPriceItem() {
  //   this.constructionPriceItem_data = [];
  //   let constructionPricies = this.validateForm.value.constructionPricies;

  //   console.log(constructionPricies);
  //   return;

  //   if (constructionPricies) {
  //     constructionPricies.forEach((item: any) => {
  //       let constructionPrice = this.constructionPricies_data.find((x: any) => x.Id == item.Id || x.Id == item.ConstructionPriceId);
  //       if (constructionPrice) this.constructionPriceItem_data = this.constructionPriceItem_data.concat(constructionPrice);
  //     });
  //   }
  // }

  async getPriceListItems() {
    const resp = await this.priceListRepository.getPriceListItems();

    if (resp.meta?.error_code == 200) {
      this.price_list_item_data = resp.data;
    }
  }

  async getLandPrice() {
    let decreeMaps = this.block.decreeMaps;
    let districtId = this.block.District;

    this.landprice_data = [];

    if (decreeMaps && districtId) {
      let list_decree = decreeMaps.reduce((res: any, cur: any) => {
        res.push(cur.key ?? cur.DecreeType1Id);
        return res;
      }, []);

      const resp = await this.landPriceRepository.getLandPriceItemsMultiDecreeType1Id(districtId, list_decree, LandPriceType.NOC, "");
      if (resp.meta?.error_code == 200) {
        this.landprice_data = resp.data;
      }
    }
  }

  // calcLandscapeLocation(landscapeLocation: number, positionCoefficientId: number) {
  //   let str = '';
  //   let arr: any[] = [];
  //   if (landscapeLocation && positionCoefficientId) {
  //     let position_coefficient_data = this.position_coefficient_data.find(x => x.Id == positionCoefficientId);
  //     if (position_coefficient_data) {
  //       switch (true) {
  //         case landscapeLocation == 1:
  //           str = `${position_coefficient_data.LocationValue1}`;
  //           arr = [position_coefficient_data.LocationValue1];
  //           break;
  //         case landscapeLocation == 2:
  //           str = `${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
  //           arr = [position_coefficient_data.LocationValue2, position_coefficient_data.LocationValue1];
  //           break;
  //         case landscapeLocation == 3:
  //           str = `${position_coefficient_data.LocationValue3} x ${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
  //           arr = [
  //             position_coefficient_data.LocationValue3,
  //             position_coefficient_data.LocationValue2,
  //             position_coefficient_data.LocationValue1
  //           ];
  //           break;
  //         case landscapeLocation == 4:
  //           str = `${position_coefficient_data.LocationValue4} x ${position_coefficient_data.LocationValue3} x ${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
  //           arr = [
  //             position_coefficient_data.LocationValue4,
  //             position_coefficient_data.LocationValue3,
  //             position_coefficient_data.LocationValue2,
  //             position_coefficient_data.LocationValue1
  //           ];
  //           break;
  //         default:
  //           break;
  //       }
  //     }
  //   }

  //   let totalPositionCoefficient = arr.reduce((value: number, curr) => {
  //     return value * curr;
  //   }, 1);

  //   this.block.PositionCoefficientArray = arr;
  //   this.block.CalcLandscapeLocation = str;

  //   this.validateForm.get('LandPrice')?.setValue(Math.round(totalPositionCoefficient * this.block.LandPriceItemValue));
  // }

  //lấy ds hệ số vị trí tính giá đất theo nghị định
  // async getPositionCoefficient(DecreeType1Id: number, landscapeLocation: number, positionCoefficientId: number) {
  //   let paging: GetByPageModel = new GetByPageModel();
  //   paging.page_size = 0;
  //   paging.query = `DecreeType1Id=${DecreeType1Id}`;

  //   const resp = await this.positionCoefficientRepository.getByPage(paging);

  //   if (resp.meta?.error_code == 200) {
  //     this.position_coefficient_data = resp.data;
  //     // this.calcLandscapeLocation(landscapeLocation, positionCoefficientId);
  //   }
  // }

  //hàm tính Diện tích chung quy đổi, tiền đất sau khi được miễn giảm, Tổng giá bán căn nhà
  calcTotalPrice() {
    let landPrice = this.validateForm.value.LandPrice;
    let apartmentPriceRemaining = this.validateForm.value.ApartmentPriceRemaining;

    this.validateForm.get('TotalPrice')?.setValue(landPrice + apartmentPriceRemaining);
  }

  async getByDecreeAndDate() {
    this.salary_default = undefined;

    let doc = this.validateForm.value.DateCreate;

    if (doc) {
      const resp = await this.salaryCoefficientRepository.getByDecreeAndDate(doc, undefined);
      if (resp.meta?.error_code == 200) {
        this.salary_default = resp.data.Value;
      }
    }
  }

  async getConstructionPriceData() {
    this.constructionPricies_data = [];

    if (this.block.decreeMaps.length) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.query = this.block.decreeMaps.map((d: any) => 'DecreeType1Id=' + (d.key ?? d.DecreeType1Id)).join(' OR ');
      paging.order_by = "Year Asc";
      paging.page_size = 0;

      const resp = await this.constructionPriceRepository.getByPage(paging);

      if (resp.meta?.error_code == 200) {
        this.constructionPricies_data = resp.data.map((item: any) => {
          item.ConstructionPriceId = item.Id;
          return item;
        });

        if (!this.record) {
          let dateCreate = this.validateForm.value.DateCreate;
          let year = (new Date(dateCreate)).getFullYear();

          this.validateForm.get('constructionPricies')?.setValue(this.constructionPricies_data.filter(x => x.Year < year));
        }
      }
    }
  }

  changeConstructionPricies() {
    let dateCreate = this.validateForm.value.DateCreate;
    let year = (new Date(dateCreate)).getFullYear();

    this.validateForm.get('constructionPricies')?.setValue(this.constructionPricies_data.filter(x => x.Year < year));
  }
}
