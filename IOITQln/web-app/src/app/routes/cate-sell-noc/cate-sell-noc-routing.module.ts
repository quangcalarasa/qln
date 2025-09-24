import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { BlockComponent } from './block/block.component';
import { FloorComponent } from './floor/floor.component';
import { AreaComponent } from './area/area.component';
import { ApartmentComponent } from './apartment/apartment.component';
import { CurrentStateMainTextureComponent } from './currstate-maintexture/currstate-maintexture.component';
import { RatioMainTextureComponent } from './ratio-maintexture/ratio-maintexture.component';
import { PriceListComponent } from './price-list/price-list.component';
import { ConstructionPriceComponent } from './construction-price/construction-price.component';
import { UseValueCoefficientComponent } from './use-value-coefficient/use-value-coefficient.component';
import { SalaryCoefficientComponent } from './salary-coefficient/salary-coefficient.component';
import { DeductionCoefficientComponent } from './deduction-coefficient/deduction-coefficient.component';
import { InvestmentRateComponent } from './investment-rate/investment-rate.component';
import { AreaCorrectionCoefficientComponent } from './area-correction-coefficient/area-correction-coefficient.component';
import { No2LandPriceComponent } from './no2-land-price/no2-land-price.component';
import { DistributionFloorCoefficientComponent } from './distribution-floor-coefficient/distribution-floor-coefficient.component';
import { PositionCoefficientComponent } from './position-coefficient/position-coefficient.component';
import { LandPriceCorrectionCoefficientComponent } from './landprice-correction-coefficient/landprice-correction-coefficient.component';
import { DeductionLandMoneyComponent } from './deduction-land-money/deduction-land-money.component';
import { LandSpecialCoefficientComponent } from './land-special-coefficient/land-special-coefficient.component';
import { LandscapeLimitComponent } from './landscape-limit/landscape-limit.component';
import { TypeBlockComponent } from './type-block/type-block.component';

const routes: Routes = [
  { path: 'apartment-info/block', component: BlockComponent, data: { title: 'Căn nhà' } },
  { path: 'apartment-info/floor', component: FloorComponent, data: { title: 'Tầng nhà' } },
  { path: 'apartment-info/area', component: AreaComponent, data: { title: 'Thông tin tầng cụ thể' } },
  { path: 'apartment-info/apartment', component: ApartmentComponent, data: { title: 'Căn hộ' } },
  { path: 'apartment-info/type-block', component: TypeBlockComponent, data: { title: 'Loại nhà' } },
  { path: 'calc-price-apartment/ct-maintexture', component: CurrentStateMainTextureComponent, data: { title: 'Hiện trạng kết cấu chính' } },
  { path: 'calc-price-apartment/ratio-maintexture', component: RatioMainTextureComponent, data: { title: 'Tỷ lệ giá trị kết cấu chính' } },
  { path: 'calc-price-apartment/price-list', component: PriceListComponent, data: { title: 'Bảng giá nhà' } },
  {
    path: 'calc-price-apartment/construction-price',
    component: ConstructionPriceComponent,
    data: { title: 'Chỉ số giá xây dựng công trình' }
  },
  {
    path: 'calc-price-apartment/use-value-coefficient',
    component: UseValueCoefficientComponent,
    data: { title: 'Hệ số điều chỉnh giá trị sử dụng' }
  },
  { path: 'calc-price-apartment/salary-coefficient', component: SalaryCoefficientComponent, data: { title: 'Hệ số lương cơ bản' } },
  {
    path: 'calc-price-apartment/deduction-coefficient',
    component: DeductionCoefficientComponent,
    data: { title: 'Hệ số được miễn giảm tiền nhà' }
  },
  { path: 'calc-price-apartment/investment-rate', component: InvestmentRateComponent, data: { title: 'Suất vốn đầu tư' } },
  {
    path: 'calc-price-apartment/area-correction-coefficient',
    component: AreaCorrectionCoefficientComponent,
    data: { title: 'Hệ số điều chỉnh vùng' }
  },
  { path: 'calc-price-land/no2-land-price', component: No2LandPriceComponent, data: { title: 'Bảng giá đất số 2' } },
  {
    path: 'calc-price-land/distribution-floor-coefficient',
    component: DistributionFloorCoefficientComponent,
    data: { title: 'Hệ số phân bổ các tầng' }
  },
  { path: 'calc-price-land/position-coefficient', component: PositionCoefficientComponent, data: { title: 'Hệ số vị trí' } },
  {
    path: 'calc-price-land/landprice-correction-coefficient',
    component: LandPriceCorrectionCoefficientComponent,
    data: { title: 'Hệ số K điều chỉnh giá đất' }
  },
  { path: 'calc-price-land/deduction-land-money', component: DeductionLandMoneyComponent, data: { title: 'Tiền đất miễn giảm' } },
  {
    path: 'calc-price-land/land-special-coefficient',
    component: LandSpecialCoefficientComponent,
    data: { title: 'Hệ số khu đất, thửa đất có hình dáng đặc biệt' }
  },
  { path: 'calc-price-land/landscape-limit', component: LandscapeLimitComponent, data: { title: 'Hạn mức đất ở' } },
  { path: 'pricing', loadChildren: () => import('./pricing/pricing.module').then(m => m.PricingModule) }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CateSellNocRoutingModule {}
