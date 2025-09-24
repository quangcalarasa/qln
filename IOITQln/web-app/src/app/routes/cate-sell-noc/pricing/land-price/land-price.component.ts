import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup, FormBuilder } from '@angular/forms';
import { LocationResidentialLand } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
import { PricingLandPriceItemComponent } from '../land-price-item/land-price-item.component';
import { AdjacentLandComponent } from '../adjacent-land/adjacent-land.component';

@Component({
  selector: 'app-pricing-land-price',
  templateUrl: './land-price.component.html'
})
export class LandPriceComponent implements OnInit {
  @ViewChild('pricingLandPriceItemComponent') pricingLandPriceItemComponent!: PricingLandPriceItemComponent;
  @ViewChild('adjacentLandComponent') adjacentLandComponent!: AdjacentLandComponent;

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() validateForm: FormGroup;
  @Input() block: any;
  @Input() apartment: any;
  @Input() landprice_data: any[] = [];

  area_data: any[] = [];
  floor_data: any[] = [];


  landscapelocation_data = LocationResidentialLand;

  validateFormLanscapePositionInfo: FormGroup;

  TypeReportApplyEnum = TypeReportApplyEnum;

  constructor(private fb: FormBuilder, private areaRepository: AreaRepository,
    private floorRepository: FloorRepository) { }

  ngOnInit(): void {
    this.validateFormLanscapePositionInfo = this.fb.group({
      decreeMaps: [this.block ? this.block.decreeMaps : [], []],
      LandPriceItemId_99: [{ value: this.block ? this.block.LandPriceItemId_99 : undefined, disabled: true }, []],
      LandPriceItemValue_99: [this.block ? this.block.LandPriceItemValue_99 : undefined, []],
      LandPriceItemId_34: [{ value: this.block ? this.block.LandPriceItemId_34 : undefined, disabled: true }, []],
      LandPriceItemValue_34: [this.block ? this.block.LandPriceItemValue_34 : undefined, []],
      LandPriceItemId_61: [{ value: this.block ? this.block.LandPriceItemId_61 : undefined, disabled: true }, []],
      LandPriceItemValue_61: [this.block ? this.block.LandPriceItemValue_61 : undefined, []],
      PositionCoefficientId_99: [{ value: this.block ? this.block.PositionCoefficientId_99 : undefined, disabled: true }, []],
      PositionCoefficientStr_99: [this.block ? this.block.PositionCoefficientStr_99 : undefined, []],
      LandscapeLocation_99: [{ value: this.block ? (this.block.LandscapeLocation_99 ? this.block.LandscapeLocation_99.toString() : undefined) : undefined, disabled: true }, []],
      LandPriceRefinement_99: [{ value: this.block ? this.block.LandPriceRefinement_99 : undefined, disabled: true }, []],
      LandScapePrice_99: [this.block ? this.block.LandScapePrice_99 : undefined, []],
      PositionCoefficientId_34: [{ value: this.block ? this.block.PositionCoefficientId_34 : undefined, disabled: true }, []],
      PositionCoefficientStr_34: [this.block ? this.block.PositionCoefficientStr_34 : undefined, []],
      LandscapeLocation_34: [{ value: this.block ? (this.block.LandscapeLocation_34 ? this.block.LandscapeLocation_34.toString() : undefined) : undefined, disabled: true }, []],
      LandPriceRefinement_34: [{ value: this.block ? this.block.LandPriceRefinement_34 : undefined, disabled: true }, []],
      LandScapePrice_34: [this.block ? this.block.LandScapePrice_34 : undefined, []],
      PositionCoefficientId_61: [{ value: this.block ? this.block.PositionCoefficientId_61 : undefined, disabled: true }, []],
      PositionCoefficientStr_61: [this.block ? this.block.PositionCoefficientStr_61 : undefined, []],
      LandscapeLocation_61: [{ value: this.block ? (this.block.LandscapeLocation_61 ? this.block.LandscapeLocation_61.toString() : undefined) : undefined, disabled: true }, []],
      LandPriceRefinement_61: [{ value: this.block ? this.block.LandPriceRefinement_61 : undefined, disabled: true }, []],
      LandScapePrice_61: [this.block ? this.block.LandScapePrice_61 : undefined, []],
      LevelAlley_34: [{ value: this.block ? (this.block.LevelAlley_34 ? this.block.LevelAlley_34.toString() : undefined) : undefined, disabled: true }, []],
      LandscapeLocationInAlley_34: [{ value: this.block ? (this.block.LandscapeLocationInAlley_34 ? this.block.LandscapeLocationInAlley_34.toString() : undefined) : undefined, disabled: true }, []],
      IsAlley_34: [{ value: this.block ? this.block.IsAlley_34 : undefined, disabled: true }, []],
      AlleyPositionCoefficientId_34: [{ value: this.block ? this.block.AlleyPositionCoefficientId_34 : undefined, disabled: true }, []],
      AlleyPositionCoefficientStr_34: [this.block ? this.block.AlleyPositionCoefficientStr_34 : undefined, []],
      AlleyLandScapePrice_34: [this.block ? this.block.AlleyLandScapePrice_34 : undefined, []],
      TextBasedInfo: [{ value: this.block ? this.block.TextBasedInfo : undefined, disabled: true }, []],
      CaseApply_34: [this.block ? (this.block.CaseApply_34 ? this.block.CaseApply_34.toString() : undefined) : "2", []],
      LandSpecial: [this.block ? this.block.LandSpecial : undefined, []],
      LandAreaSpecial: [{ value: this.block ? this.block.LandAreaSpecial : undefined, disabled: true }, []],
      LandAreaSpecialS1: [{ value: this.block ? this.block.LandAreaSpecialS1 : undefined, disabled: true }, []],
      LandAreaSpecialS2: [{ value: this.block ? this.block.LandAreaSpecialS2 : undefined, disabled: true }, []],
      LandAreaSpecialS3: [{ value: this.block ? this.block.LandAreaSpecialS3 : undefined, disabled: true }, []],
      WidthSpecial: [{ value: this.block ? this.block.WidthSpecial : undefined, disabled: true }, []],
      WidthSpecialS1: [{ value: this.block ? this.block.WidthSpecialS1 : undefined, disabled: true }, []],
      WidthSpecialS2: [{ value: this.block ? this.block.WidthSpecialS2 : undefined, disabled: true }, []],
      WidthSpecialS3: [{ value: this.block ? this.block.WidthSpecialS3 : undefined, disabled: true }, []],
      LandPositionSpecial: [{ value: this.block ? this.block.LandPositionSpecial : undefined, disabled: true }, []],
      TypeBlockEntity: [{ value: this.block ? this.block.TypeBlockEntity : undefined, disabled: true }, []],
      IsFrontOfLine_61: [{ value: this.block ? this.block.IsFrontOfLine_61 : true, disabled: true }, []],
      TypeAlley_61: [{ value: this.block ? (this.block.TypeAlley_61 ? this.block.TypeAlley_61.toString() : undefined) : undefined, disabled: true }, []],
      LandscapeLocationInAlley_61: [{ value: this.block ? (this.block.LandscapeLocationInAlley_61 ? this.block.LandscapeLocationInAlley_61.toString() : undefined) : undefined, disabled: true }, []],
      IsAlley_61: [{ value: this.block ? this.block.IsAlley_61 : undefined, disabled: true }, []],
      No2LandScapePrice_61: [this.block ? this.block.No2LandScapePrice_61 : undefined, []],
      Width: [{ value: this.block ? this.block.Width : undefined, disabled: true }, []],
      Deep: [{ value: this.block ? this.block.Deep : undefined, disabled: true }, []],
      ExceedingLimitDeep: [{ value: this.block ? this.block.ExceedingLimitDeep : undefined, disabled: true }, []],
      Disabled: [{ value: true }, []],
      IsFrontOfLine_61_Clone: [{ value: this.block ? this.block.IsFrontOfLine_61 : true }, []]
    });

    if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
      this.getDataArea();
      this.getDataFloor();
    }
  }

  calcTotalPrice() {
    // Call emit
    this.eventEmitter.emit();
  }

  async getDataArea() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `1=1`;
    paging.page_size = 0;
    paging.select = 'Id,Code,Name,FloorId,IsMezzanine';
    paging.order_by = 'Id Asc';

    const resp = await this.areaRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.area_data = resp.data;
    }
  }

  async getDataFloor() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = '1=1';
    paging.select = 'Id,Code,Name';
    paging.order_by = 'Id Asc';

    const resp = await this.floorRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.floor_data = resp.data;
    }
  }

  changeFlatCoefficient() {
    if (this.pricingLandPriceItemComponent)
      this.pricingLandPriceItemComponent.changeFlatCoefficient();
  }

  adjacentLandChange(invalid: boolean) {
    if (!invalid) {
      let data = this.adjacentLandComponent.tableItemRef._data;

      this.pricingLandPriceItemComponent.calcLandPriceBbl5(data);
    }
    else {

    }
  }
}
