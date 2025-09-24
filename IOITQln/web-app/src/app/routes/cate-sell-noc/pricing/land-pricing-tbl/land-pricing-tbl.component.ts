import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { Decree, TermApply } from 'src/app/shared/utils/consts';
import { DecreeEnum, TermApplyEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { InvestmentRateRepository } from 'src/app/infrastructure/repositories/investment-rate.repository';



@Component({
  selector: 'app-land-pricing-tbl',
  templateUrl: './land-pricing-tbl.component.html'
})
export class LandPricingTblComponent implements OnInit {
  @Input() curr_date: any;
  @Input() apartmentDetailData: any[] = [];
  @Input() blockMaintextureRaties: any[] = [];
  @Input() apartmentMaintextureRaties: any[] = [];
  @Input() price_list_item_data: any[] = [];
  @Input() constructionPricies_data: any[] = [];
  @Input() type_pile: number = 1;
  @Input() typeReportApply: number;
  @Input() parentTypeReportApply: number;
  @Input() areaCorrectionCoefficientValue: number;

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];

  investment_rate_item_data: any[] = [];

  TypeReportApplyEnum = TypeReportApplyEnum;
  TermApplyEnum = TermApplyEnum;

  tblInVestmentRateValid = true;
  tblUnInVestmentRateValid = true;

  constructor(
    private investmentRateRepository: InvestmentRateRepository
  ) { }

  ngOnInit(): void {
    if (!this.data.length) this.initData();
    else {
      let apartmentDetailData = this.data.map(item => {
        let floor = this.apartmentDetailData.find(x => x.FloorId == item.FloorId);
        item.FloorName = floor ? floor.FloorName : "";

        let area = this.apartmentDetailData.find(x => x.AreaId == item.AreaId);
        item.AreaName = area ? area.AreaName : "";

        item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
        item.TermApplyName = TermApply[item.TermApply as unknown as keyof typeof TermApply];

        return item;
      });

      this.apartmentDetailData = [...apartmentDetailData];
    }

    if (this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU || (this.typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.parentTypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU)) {
      this.getInvestmentRateItems();

      //Kiểm tra xem
      let investment = 
      this.tblInVestmentRateValid = this.apartmentDetailData.some(x => x.ApplyInvestmentRate && x.DecreeType1Id == DecreeEnum.ND_99 && !x.IsMezzanine);
      this.tblUnInVestmentRateValid = this.apartmentDetailData.some(x => !x.ApplyInvestmentRate || x.DecreeType1Id != DecreeEnum.ND_99 || x.IsMezzanine);
    }
  }

  initData() {
    this.apartmentDetailData.forEach(item => {
      let blockMaintextureRate = this.blockMaintextureRaties.find(x => x.LevelBlockId == item.Level);
      let apartmentMaintextureRate = this.apartmentMaintextureRaties.find(x => x.LevelBlockId == item.Level);

      if (apartmentMaintextureRate) item.MaintextureRateValue = apartmentMaintextureRate.TotalValue;
      else item.MaintextureRateValue = blockMaintextureRate ? blockMaintextureRate.TotalValue : undefined;

      item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
      item.TermApplyName = TermApply[item.TermApply as unknown as keyof typeof TermApply];
    });
  }

  changePriceListItem(PriceListItemId: number, emit: boolean, item: any) {
    let priceListItem = this.price_list_item_data.find(x => x.Id == PriceListItemId);
    if (priceListItem) {
      let price = this.type_pile == 1 ? priceListItem.ValueTypePile1 : priceListItem.ValueTypePile2;
      item.Price = price;

      if (item.DecreeType1Id == DecreeEnum.ND_99) {

        let priceInYear = this.constructionPricies_data.reduce((price: number, curValue) => {
          return (price * curValue.Value / 100);
        }, price);

        item.PriceInYear = Math.round(priceInYear);
      }
      else if (item.DecreeType1Id == DecreeEnum.ND_34) {
        item.PriceInYear = price;
      }
      else {
        item.PriceInYear = price;
      }

      let maintextureRateValue = item.MaintextureRateValue ?? 100;

      if (item.TermApply == TermApplyEnum.DIEU_65 || item.TermApply == TermApplyEnum.KHOAN_1_DIEU_34 || item.TermApply == TermApplyEnum.DIEU_7) {
        if (maintextureRateValue && item.CoefficientUseValue) {
          item.RemainingPrice = Math.round(item.PriceInYear * maintextureRateValue * item.CoefficientUseValue * ((item.GeneralArea ?? 0) + (item.PrivateArea ?? 0)) / 100);
        }
        else {
          item.RemainingPrice = undefined;
        }
      }
      else {
        if (maintextureRateValue) {
          item.RemainingPrice = Math.round(item.PriceInYear * maintextureRateValue * ((item.GeneralArea ?? 0) + (item.PrivateArea ?? 0)) / 100);
        }
        else {
          item.RemainingPrice = undefined;
        }
      }
    }
    else {
      item.Price = undefined;
      item.PriceInYear = undefined;
      item.RemainingPrice = undefined;
    }

    if (emit) {
      this.data = [...this.apartmentDetailData];
      this.eventEmitter.emit(this.data);
    }
  }

  customPriceListItemSelectedlabel(label: string) {
    if (label) {
      return label.split(" + ")[0];
    }
    else return "";
  }

  //Tính lại giá trị các row khi chỉ số giá xây dựng thay đổi
  recalculateValues() {
    this.apartmentDetailData.forEach((item, index) => {
      if (item.DecreeType1Id == DecreeEnum.ND_99 && item.ApplyInvestmentRate && (this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU || (this.typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.parentTypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU))) {
        let investmentRateItemId = item.InvestmentRateItemId;
        this.changeInvestmentRateItem(investmentRateItemId, false, item);
      }
      else {
        let priceListItemId = item.PriceListItemId;
        this.changePriceListItem(priceListItemId, false, item);
      }
    });

    this.data = [...this.apartmentDetailData];
    this.eventEmitter.emit(this.data);
  }

  customInvestmentRateItemSelectedlabel(label: string) {
    if (label) {
      return label.split(",")[0];
    }
    else return "";
  }

  async getInvestmentRateItems() {
    const resp = await this.investmentRateRepository.getInvestmentRateItems(TypeReportApplyEnum.NHA_CHUNG_CU);

    if (resp.meta?.error_code == 200) {
      this.investment_rate_item_data = resp.data;
    }
  }

  changeInvestmentRateItem(InvestmentRateItemId: number, emit: boolean, item: any) {
    let investmentRateItem = this.investment_rate_item_data.find(x => x.Id == InvestmentRateItemId);
    if (investmentRateItem) {
      let price = investmentRateItem.Value - investmentRateItem.Value2;
      item.Price = price;
      item.InvestmentRateValue = investmentRateItem.Value;
      item.InvestmentRateValue1 = investmentRateItem.Value1;
      item.InvestmentRateValue2 = investmentRateItem.Value2;

      if (item.DecreeType1Id == DecreeEnum.ND_99) {
        let priceInYear = this.constructionPricies_data.reduce((price: number, curValue) => {
          return Math.round(price * curValue.Value / 100);
        }, price);

        item.PriceInYear = Math.round(priceInYear * (this.areaCorrectionCoefficientValue ?? 1));
      }

      let maintextureRateValue = item.MaintextureRateValue ?? 100;

      if (item.TermApply == TermApplyEnum.DIEU_65) {
        if (maintextureRateValue && item.CoefficientUseValue && item.PrivateArea) {
          item.RemainingPrice = Math.round(item.PriceInYear * maintextureRateValue * item.CoefficientUseValue * (item.PrivateArea ?? 0) / 100);
        }
        else {
          item.RemainingPrice = undefined;
        }
      }
      else if (item.TermApply == TermApplyEnum.DIEU_70) {
        if (maintextureRateValue && item.PrivateArea) {
          item.RemainingPrice = Math.round(item.PriceInYear * maintextureRateValue * (item.PrivateArea ?? 0) / 100);
        }
        else {
          item.RemainingPrice = undefined;
        }
      }
      else if (item.TermApply == TermApplyEnum.DIEU_71 || item.TermApply == TermApplyEnum.DIEU_35) {
        if (maintextureRateValue && item.PrivateArea) {
          item.RemainingPrice = Math.round(item.PriceInYear * maintextureRateValue * (item.PrivateArea ?? 0) / 100);
        }
        else {
          item.RemainingPrice = undefined;
        }
      }
    }
    else {
      item.Price = undefined;
      item.PriceInYear = undefined;
      item.RemainingPrice = undefined;
    }

    if (emit) {
      this.data = [...this.apartmentDetailData];
      this.eventEmitter.emit(this.data);
    }
  }
}
