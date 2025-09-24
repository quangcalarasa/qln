import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CateSellNocRoutingModule } from './cate-sell-noc-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';
import { FilterTermApplyByDecreePipe } from 'src/app/shared/pipe/filter-termapply-by-decree.pipe';

import { BlockComponent } from './block/block.component';
import { AddOrUpdateBlockComponent } from './block/add-or-update/add-or-update-block.component';
import { ChooseAreaBlockComponent } from './block/choose-area/change-area-block.component';
import { MainTextureRateTblComponent } from './block/maintexture-rate-tbl/maintexture-rate-tbl.component';
import { BlockDetailComponent } from './block/block-detail/block-detail.component';
import { BlockAddressComponent } from './block/block-address/block-address.component';
import { ApplyRangeComponent } from './block/apply-range/apply-range.component';
import { BlockInfoComponent } from './block/block-info/block-info.component';
import { BlockAreaComponent } from './block/block-area/block-area.component';
import { LandscapeInfoComponent } from './block/landscape-info/landscape-info.component';
import { LandscapeAreaInfoComponent } from './block/landscape-area-info/landscape-area-info.component';
// import { BlockLandSpecialComponent } from './block/land-special/land-special.component';
// import { LandscapePositionComponent } from './block/landscape-position/landscape-position.component';
// import { LandscapePositionInfoComponent } from './block/landscape-position-info/landscape-position-info.component';
import { SearchRentBlockComponent } from './block/search-rent-block/search-rent-block.component';

import { FloorComponent } from './floor/floor.component';
import { AddOrUpdateFloorComponent } from './floor/add-or-update/add-or-update-floor.component';

import { AreaComponent } from './area/area.component';
import { AddOrUpdateAreaComponent } from './area/add-or-update/add-or-update-area.component';

import { ApartmentComponent } from './apartment/apartment.component';
import { AddOrUpdateApartmentComponent } from './apartment/add-or-update/add-or-update-apartment.component';
import { ApartmentDetailComponent } from './apartment/apartment-detail/apartment-detail.component';
import { ApartmentLandDetailComponent } from './apartment/apartment-land-detail/apartment-land-detail.component';
import { SearchRentApartmentComponent } from './apartment/search-rent-apartment/search-rent-apartment.component';
import { ApartmentRangeComponent } from './apartment/apartment-range/apartment-range.component';
import { ApartmentInfoComponent } from './apartment/apartment-info/apartment-info.component';

import { CurrentStateMainTextureComponent } from './currstate-maintexture/currstate-maintexture.component';
import { AddOrUpdateCurrentStateMainTextureComponent } from './currstate-maintexture/add-or-update/add-or-update-currstate-maintexture.component';

import { RatioMainTextureComponent } from './ratio-maintexture/ratio-maintexture.component';
import { AddOrUpdateRatioMainTextureComponent } from './ratio-maintexture/add-or-update/add-or-update-ratio-maintexture.component';

import { PriceListComponent } from './price-list/price-list.component';
import { AddOrUpdatePricelistComponent } from './price-list/add-or-update/add-or-update-price-list.component';

import { ConstructionPriceComponent } from './construction-price/construction-price.component';
import { AddOrUpdateConstructionPriceComponent } from './construction-price/add-or-update/add-or-update-construction-price.component';

import { UseValueCoefficientComponent } from './use-value-coefficient/use-value-coefficient.component';
import { AddOrUpdateUseValueCoefficientComponent } from './use-value-coefficient/add-or-update/add-or-update-use-value-coefficient.component';

import { SalaryCoefficientComponent } from './salary-coefficient/salary-coefficient.component';
import { AddOrUpdateSalaryCoefficientComponent } from './salary-coefficient/add-or-update/add-or-update-salary-coefficient.component';

import { DeductionCoefficientComponent } from './deduction-coefficient/deduction-coefficient.component';
import { AddOrUpdateDeductionCoefficientComponent } from './deduction-coefficient/add-or-update/add-or-update-deduction-coefficient.component';

import { InvestmentRateComponent } from './investment-rate/investment-rate.component';
import { AddOrUpdateInvestmentRateComponent } from './investment-rate/add-or-update/add-or-update-investment-rate.component';

import { AreaCorrectionCoefficientComponent } from './area-correction-coefficient/area-correction-coefficient.component';
import { AddOrUpdateAreaCorrectionCoefficientComponent } from './area-correction-coefficient/add-or-update/add-or-update-area-correction-coefficient.component';

import { No2LandPriceComponent } from './no2-land-price/no2-land-price.component';
import { AddOrUpdateNo2LandPriceComponent } from './no2-land-price/add-or-update/add-or-update-no2-land-price.component';

import { DistributionFloorCoefficientComponent } from './distribution-floor-coefficient/distribution-floor-coefficient.component';
import { AddOrUpdateDistributionFloorCoefficientComponent } from './distribution-floor-coefficient/add-or-update/add-or-update-distribution-floor-coefficient.component';

import { PositionCoefficientComponent } from './position-coefficient/position-coefficient.component';
import { AddOrUpdatePositionCoefficientComponent } from './position-coefficient/add-or-update/add-or-update-position-coefficient.component';

import { LandPriceCorrectionCoefficientComponent } from './landprice-correction-coefficient/landprice-correction-coefficient.component';
import { AddOrUpdateLandPriceCorrectionCoefficientComponent } from './landprice-correction-coefficient/add-or-update/add-or-update-landprice-correction-coefficient.component';

import { DeductionLandMoneyComponent } from './deduction-land-money/deduction-land-money.component';
import { AddOrUpdateDeductionLandMoneyComponent } from './deduction-land-money/add-or-update/add-or-update-deduction-land-money.component';

import { LandSpecialCoefficientComponent } from './land-special-coefficient/land-special-coefficient.component';
import { AddOrUpdateLandSpecialCoefficientComponent } from './land-special-coefficient/add-or-update/add-or-update-land-special-coefficient.component';

//Danh mục hạn mức đất ở
import { LandscapeLimitComponent } from './landscape-limit/landscape-limit.component';
import { AddOrUpdateLandscapeLimitComponent } from './landscape-limit/add-or-update/add-or-update-landscape-limit.component';

import { FilesComponent } from './files/files.component';
import { AddOrUpdateFilesComponent } from './files/add-or-update-files/add-or-update-files.component';

import { TypeBlockComponent } from './type-block/type-block.component';
import { AddOrUpdateTypeBlockComponent } from './type-block/add-or-update/add-or-update-type-block.component';

import { NocBlockFileComponent } from './block/noc-block-file/noc-block-file.component';
import { NocApartmentFileComponent } from './apartment/noc-apartment-file/noc-apartment-file.component';

const COMPONENTS: Array<Type<void>> = [
  BlockComponent,
  AddOrUpdateBlockComponent,
  FloorComponent,
  AddOrUpdateFloorComponent,
  AreaComponent,
  AddOrUpdateAreaComponent,
  ApartmentComponent,
  AddOrUpdateApartmentComponent,
  SearchRentApartmentComponent,
  ApartmentDetailComponent,
  ApartmentLandDetailComponent,
  ApartmentRangeComponent,
  ApartmentInfoComponent,
  CurrentStateMainTextureComponent,
  AddOrUpdateCurrentStateMainTextureComponent,
  RatioMainTextureComponent,
  AddOrUpdateRatioMainTextureComponent,
  PriceListComponent,
  AddOrUpdatePricelistComponent,
  ConstructionPriceComponent,
  AddOrUpdateConstructionPriceComponent,
  UseValueCoefficientComponent,
  AddOrUpdateUseValueCoefficientComponent,
  SalaryCoefficientComponent,
  AddOrUpdateSalaryCoefficientComponent,
  DeductionCoefficientComponent,
  AddOrUpdateDeductionCoefficientComponent,
  InvestmentRateComponent,
  AddOrUpdateInvestmentRateComponent,
  AreaCorrectionCoefficientComponent,
  AddOrUpdateAreaCorrectionCoefficientComponent,
  DistributionFloorCoefficientComponent,
  AddOrUpdateDistributionFloorCoefficientComponent,
  PositionCoefficientComponent,
  AddOrUpdatePositionCoefficientComponent,
  LandPriceCorrectionCoefficientComponent,
  AddOrUpdateLandPriceCorrectionCoefficientComponent,
  DeductionLandMoneyComponent,
  AddOrUpdateDeductionLandMoneyComponent,
  LandSpecialCoefficientComponent,
  AddOrUpdateLandSpecialCoefficientComponent,
  ChooseAreaBlockComponent,
  MainTextureRateTblComponent,
  BlockDetailComponent,
  BlockAddressComponent,
  ApplyRangeComponent,
  BlockInfoComponent,
  BlockAreaComponent,
  LandscapeInfoComponent,
  LandscapeAreaInfoComponent,
  SearchRentBlockComponent,
  No2LandPriceComponent,
  AddOrUpdateNo2LandPriceComponent,
  LandscapeLimitComponent,
  AddOrUpdateLandscapeLimitComponent,
  FilesComponent,
  AddOrUpdateFilesComponent,
  TypeBlockComponent,
  AddOrUpdateTypeBlockComponent,
  NocBlockFileComponent,
  NocApartmentFileComponent,
];

@NgModule({
  imports: [
    SharedModule,
    CateSellNocRoutingModule,
    NzPageHeaderModule,
    NzTreeSelectModule,
    NzUploadModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
    NzCalendarModule,
    NzTreeModule,
    PipeModule
  ],
  declarations: COMPONENTS,
  providers: [DatePipe, KeyValuePipe, FilterTermApplyByDecreePipe]
})
export class CateSellNocModule {}
