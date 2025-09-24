import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { LevelBlock } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { ReducedPersonComponent } from '../reduced-person/reduced-person.component';
import { LandPricingTblComponent } from '../land-pricing-tbl/land-pricing-tbl.component';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { AreaCorrectionCoefficientRepository } from 'src/app/infrastructure/repositories/area-correction-coefficient.repository';

@Component({
  selector: 'app-pricing-house-price',
  templateUrl: './house-price.component.html'
})
export class HousePriceComponent implements OnInit {
  @ViewChild('reducedPersonComponent') private reducedPersonComponent!: ReducedPersonComponent;
  @ViewChild('landPricingTblComponent') private landPricingTblComponent!: LandPricingTblComponent;

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() validateForm: FormGroup;
  @Input() block: any;
  @Input() apartment: any;
  @Input() typehouse_data: NzSafeAny;
  @Input() vat_data: NzSafeAny;
  @Input() customer_data: any[] = [];
  @Input() salary_default?: number = undefined;
  @Input() deduction_coefficient_data: any[] = [];
  @Input() constructionPricies_data: any[] = [];
  @Input() price_list_item_data: any[] = [];

  invalidTblReducedPerson = false;

  TypeReportApplyEnum = TypeReportApplyEnum;

  area_correction_coefficient_data: any[] = [];

  constructor(
    private areaCorrectionCoefficientRepository: AreaCorrectionCoefficientRepository
  ) { }

  ngOnInit(): void {
    this.getDeductionCoefficientData();
  }

  genLevelColumn(levelBlockMaps: any) {
    let res = '';

    levelBlockMaps.forEach((item: any) => {
      Object.keys(LevelBlock).some(v => {
        if (v === item.LevelId.toString()) {
          res =
            res == ''
              ? LevelBlock[v as unknown as keyof typeof LevelBlock]
              : res + ' + ' + LevelBlock[v as unknown as keyof typeof LevelBlock];
        }
      });
    });

    return res;
  }

  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find((x: any) => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.ConstructionPriceId === o2.Id : o1 === o2;
  };

  changeVat(vatId: number) {
    if (vatId) {
      let vat = this.vat_data.find((x: any) => x.Id == vatId);

      this.validateForm.get('Vat')?.setValue(vat ? vat.Value : undefined);
    } else this.validateForm.get('Vat')?.setValue(undefined);

    this.calcApartmentPrice();
    this.calcConversionArea();
  }

  changeAreaCorrectionCoefficient(id: number) {
    if (id) {
      let accd = this.area_correction_coefficient_data.find((x: any) => x.Id == id);

      this.validateForm.get('AreaCorrectionCoefficientValue')?.setValue(accd ? accd.Value : undefined);
    } else this.validateForm.get('AreaCorrectionCoefficientValue')?.setValue(undefined);
  }

  changeLandPricingTbl(landPricingTbl: any) {
    this.validateForm.get('landPricingTbl')?.setValue(landPricingTbl);

    let apartmentPrice = landPricingTbl.reduce((total: number, currValue: any) => {
      return total + (currValue.RemainingPrice ?? 0);
    }, 0);

    this.validateForm.get('ApartmentPrice')?.setValue(apartmentPrice);
    this.calcApartmentPrice();
    this.calcConversionArea();
  }

  changeReducedPerson(invalidTblReducedPerson: boolean) {
    this.invalidTblReducedPerson = invalidTblReducedPerson;

    if (!this.invalidTblReducedPerson) {
      let reducedPerson = this.reducedPersonComponent.tableItemRef._data;
      this.validateForm.get('reducedPerson')?.setValue(reducedPerson);

      let apartmentPriceReduced = reducedPerson.reduce((total: number, currValue: any) => {
        return total + (currValue.Value ?? 0);
      }, 0);

      this.validateForm.get('ApartmentPriceReduced')?.setValue(apartmentPriceReduced);
      this.calcApartmentPrice();
      this.calcConversionArea();
    }
  }

  calcApartmentPrice() {
    let apartmentPrice = this.validateForm.value.ApartmentPrice;
    let apartmentPriceReduced = this.validateForm.value.ApartmentPriceReduced;
    let vat = this.validateForm.value.Vat;

    if (apartmentPrice != undefined) {
      let apartmentPriceRemaining = apartmentPrice - (apartmentPriceReduced ?? 0);
      this.validateForm.get('ApartmentPriceRemaining')?.setValue(apartmentPriceRemaining);

      let apartmentPriceNoVat = Math.round((apartmentPriceRemaining * 100) / (100 + vat ?? 0));
      this.validateForm.get('ApartmentPriceNoVat')?.setValue(apartmentPriceNoVat);

      this.validateForm.get('ApartmentPriceVat')?.setValue(apartmentPriceRemaining - apartmentPriceNoVat);
    }
  }

  calcConversionArea() {
    // emit tới component cha
    this.eventEmitter.emit();
  }

  async changeContructionPrice() {
    await new Promise(f => setTimeout(f, 50));
    if (this.landPricingTblComponent) this.landPricingTblComponent.recalculateValues();
  }

  async getDeductionCoefficientData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Value,Note';

    const resp = await this.areaCorrectionCoefficientRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.area_correction_coefficient_data = resp.data;
    }
  }

  genLevelApartment(apartmentDetailData: any[]) {
    let str = "";
    let lastValue: any;
    if (apartmentDetailData) {
      let arr = apartmentDetailData.reduce((result, curValue) => {
        if (lastValue != curValue.Level) {
          result.push("Cấp " + curValue.Level);
        }
        lastValue = curValue.Level;
        return result;
      }, []);

      str = arr.join(' + ');
    }

    return str;
  }

  calcSumArea(constructionAreaValue: number, sellConstructionAreaValue: number) {
    return ((Number)(constructionAreaValue ?? 0) + (Number)(sellConstructionAreaValue ?? 0));
  }
}
