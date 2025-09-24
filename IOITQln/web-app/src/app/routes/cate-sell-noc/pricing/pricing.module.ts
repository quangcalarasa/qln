import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { PricingRoutingModule } from './pricing-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';

import { PricingComponent } from './list/pricing.component';
import { InitPricingComponent } from './init-pricing/init-pricing.component';
import { AddOrUpdatePricingComponent } from './add-or-update/add-or-update-pricing.component';
import { BlockDetailComponent } from './block-detail/block-detail.component';
import { LandPricingTblComponent } from './land-pricing-tbl/land-pricing-tbl.component';
import { ReducedPersonComponent } from './reduced-person/reduced-person.component';
import { PricingOfficerComponent } from './pricing-officer/pricing-officer.component';
import { ApartmentCommonInfoComponent } from './common-info/common-info.component';
import { PricingBlockApartmentInfoComponent } from './block-apartment-info/block-apartment-info.component';
import { HousePriceComponent } from './house-price/house-price.component';
import { LandPriceComponent } from './land-price/land-price.component';
import { PricingLandPriceItemComponent } from './land-price-item/land-price-item.component';
import { FlatCoefficientComponent } from './flat-coefficient/flat-coefficient.component';
import { AdjacentLandComponent } from './adjacent-land/adjacent-land.component';

const COMPONENTS: Array<Type<void>> = [
  PricingComponent,
  InitPricingComponent,
  AddOrUpdatePricingComponent,
  BlockDetailComponent,
  LandPricingTblComponent,
  ReducedPersonComponent,
  PricingOfficerComponent,
  ApartmentCommonInfoComponent,
  PricingBlockApartmentInfoComponent,
  HousePriceComponent,
  LandPriceComponent,
  PricingLandPriceItemComponent,
  FlatCoefficientComponent,
  AdjacentLandComponent
];

@NgModule({
  imports: [
    SharedModule,
    PricingRoutingModule,
    NzPageHeaderModule,
    NzTreeSelectModule,
    NzUploadModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
    NzCalendarModule,
    NzTreeModule,
    PipeModule
  ],
  declarations: COMPONENTS,
  providers: [DatePipe]
})
export class PricingModule { }
