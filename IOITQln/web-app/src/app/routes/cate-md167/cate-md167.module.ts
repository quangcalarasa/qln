import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CateMd167RoutingModule } from './cate-md167-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';

import { LandTaxComponent } from './land-tax/land-tax.component';
import { AddOrUpdateMd167LandTaxComponent } from './land-tax/add-or-update/add-or-update.component';
import { DelegateComponent } from './delegate/delegate.component';
import { AddOrUpdateDelegateComponent } from './delegate/add-or-update/add-or-update.component';
import { LandPriceComponent } from './land-price/land-price.component';
import { AddOrUpdateMd167LandPriceComponent } from './land-price/add-or-update/add-or-update.component';
import { AuctioneerComponent } from './auctioneer/auctioneer.component';
import { AddOrUpdateMd167AuctioneerComponent } from './auctioneer/add-or-update/add-or-update.component';
import { AreaValueComponent } from './area-value/area-value.component';
import { AddorupdateAreaValueComponent } from './area-value/addorupdate/addorupdate.component';
import { VatValueComponent } from './vat-value/vat-value.component';
import { AddorupdateVatValueComponent } from './vat-value/addorupdate/addorupdate.component';
import { HouseTypeComponent } from './house-type/house-type.component';
import { AddorupdateMd167HouseTypeComponent } from './house-type/addorupdate/addorupdate.component';
import { HouseComponent } from './house/house.component';
import { AddorupdateHouseComponent } from './house/addorupdate/addorupdate.component';

import { AddorupdateMd167KiosComponent } from './house/kios/addorupdate/addorupdate.component';
import { PositionValueComponent } from './position-value/position-value.component';
import { AddOrUpdatePositionValueComponent } from './position-value/add-or-update/add-or-update.component';
import { HouseInfoComponent } from './house/house-info/house-info.component';
import { HouseProposeComponent } from './house/house-propose/house-propose.component';
import { Md167ProfitValueComponent } from './profit-value/profit-value.component';
import { AddOrUpdateMd167ProfitValueComponent } from './profit-value/add-or-update/add-or-update.component';
import { ImportExcelMd167PVComponent } from './profit-value/import-excel/import-excel.component';
import { TDCRoutingModule } from '../tdc/tdc-routing.module';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';
import { Md167ManagePurposeComponent } from './md167-manage-purpose/md167-manage-purpose.component';
import { AddOrUpdateManPurposeComponent } from './md167-manage-purpose/add-or-update/add-or-update.component';
import { StateOfUseComponent } from './state-of-use/state-of-use.component';
import { AddOrUpdateStateOfUseComponent } from './state-of-use/add-or-update/add-or-update.component';
import { KiosComponent } from './house/kios/kios.component';
import { AddOrUpdateTranferUnitComponent } from './tranfer-unit/add-or-update/add-or-update.component';
import { TranferUnitComponent } from './tranfer-unit/tranfer-unit.component';
import { AddOrUpdatePlanContentComponent } from './plant-content/add-or-update/add-or-update.component';
import { PlantContentComponent } from './plant-content/plant-content.component';

import { ManagePaymentComponent } from './manage-payment/manage-payment.component';
import { AddOrUpdateManagePaymentComponent } from './manage-payment/add-or-update/add-or-update.component';
import { HomeInfoComponent } from './manage-payment/home-info/home-info.component';
import { Md167HouseFileComponent } from './house/md167-house-file/md167-house-file.component';


const COMPONENTS: Array<Type<void>> = [
  LandTaxComponent,
  AddOrUpdateMd167LandTaxComponent,

  DelegateComponent,
  AddOrUpdateDelegateComponent,

  LandPriceComponent,
  AddOrUpdateMd167LandPriceComponent,

  AuctioneerComponent,
  AddOrUpdateMd167AuctioneerComponent,

  PositionValueComponent,
  AddOrUpdatePositionValueComponent,

  AreaValueComponent,
  AddorupdateAreaValueComponent,

  VatValueComponent,
  AddorupdateVatValueComponent,

  HouseTypeComponent,
  AddorupdateMd167HouseTypeComponent,

  HouseComponent,
  AddorupdateHouseComponent,

  AddorupdateMd167KiosComponent,
  KiosComponent,

  HouseInfoComponent,
  HouseProposeComponent,

  Md167ManagePurposeComponent,
  AddOrUpdateManPurposeComponent,

  StateOfUseComponent,
  AddOrUpdateStateOfUseComponent,

  Md167ProfitValueComponent,
  AddOrUpdateMd167ProfitValueComponent,
  ImportExcelMd167PVComponent,

  AddOrUpdateTranferUnitComponent,
  TranferUnitComponent,

  AddOrUpdatePlanContentComponent,
  PlantContentComponent,

  ManagePaymentComponent,
  AddOrUpdateManagePaymentComponent,
  HomeInfoComponent,

  Md167HouseFileComponent,
];

@NgModule({
  imports: [
    SharedModule,
    CateMd167RoutingModule,
    NzPageHeaderModule,
    NzTreeSelectModule,
    NzUploadModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
    NzCalendarModule,
    PipeModule
  ],
  declarations: COMPONENTS,
  providers: [DatePipe]
})
export class CateMd167Module { }
