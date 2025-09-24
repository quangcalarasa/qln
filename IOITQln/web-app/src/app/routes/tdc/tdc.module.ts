import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';

import { TDCRoutingModule } from './tdc-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';

import { IngredientspriceComponent } from './ingredientsprice/ingredientsprice.component';
import { AddOrUpdateIngredientsPriceComponent } from './ingredientsprice/add-or-update/add-or-update-ingredientsprice.component';

import { OriginalPriceAndTaxComponent } from './original-price-and-tax/original-price-and-tax.component';
import { AddOrUpdateOPATComponent } from './original-price-and-tax/add-or-update/add-or-update-OPAT.component';

import { ResettlementApartmentComponent } from './resettlement-apartment/resettlement-apartment.component';
import { AddOrUpdateResettlementApartmentComponent } from './resettlement-apartment/add-or-update/add-or-update-resettlement-apartment.component';

import { ProfitValueComponent } from './profit-value/profit-value.component';
import { AddOrUpdateProfitValueComponent } from './profit-value/add-or-update/add-or-update.component';

import { AnnualInstallmentComponent } from './annual-installment/annual-installment.component';
import { AddOrUpdateAnnualInstallmentComponent } from './annual-installment/add-or-update/add-or-update.component';

import { LandComponent } from './land/land.component';
import { AddOrUpdateLandComponent } from './land/add-or-update-land/add-or-update-land.component';

import { BlockHouseComponent } from './block-house/block-house.component';
import { AddOrUpdateBlockHouseComponent } from './block-house/add-or-update/add-or-update-block-house.component';

import { FloorTdcComponent } from './floor-tdc/floor-tdc.component';
import { AddOrUpdateFloorTdcComponent } from './floor-tdc/add-or-update-floortdc/add-or-update-floortdc.component';

import { ApartmentTdcComponent } from './apartment-tdc/apartment-tdc.component';
import { AddOrUpdateApartmentTdcComponent } from './apartment-tdc/add-or-update-apartmenttdc/add-or-update-apartmenttdc.component';

import { TdcProjectComponent } from './tdc-project/tdc-project.component';
import { AddOrUpdateProjectComponent } from './tdc-project/add-or-update/add-or-update-project.component';
import { TdcProjectIngrepriceComponent } from './tdc-project/tdc-project-ingreprice/tdc-project-ingreprice.component';
import { TdcProjectPriceAndTaxComponent } from './tdc-project/tdc-project-price-and-tax/tdc-project-price-and-tax.component';
import { PlatformTdcComponent } from './platform-tdc/platform-tdc.component';
import { AddOrUpdatePlatformTdcComponent } from './platform-tdc/add-or-update-platformtdc/add-or-update-platformtdc.component';

import { AddOrUpdateCsComponent } from './tdc-customer/add-or-update-cs/add-or-update-cs.component';
import { TdcCustomerFileComponent } from './tdc-customer/tdc-customer-file/tdc-customer-file.component';
import { TdcCustomerComponent } from './tdc-customer/tdc-customer.component';

import { TdcReportApartmentComponent } from './tdc-report-apartment/tdc-report-apartment.component';
import { AddOrUpdateApartmentReportComponent } from './tdc-report-apartment/add-or-update/add-or-update.component';

import { TdcReportPlatformComponent } from './tdc-report-platform/tdc-report-platform.component';
import { AddOrUpdatePlatformReportComponent } from './tdc-report-platform/add-or-update/add-or-update.component';

import { Report2AllComponent } from './report2-all/report2-all.component';
import { TdcMemberComponent } from './tdc-customer/tdc-member/tdc-member.component';
import { TdcAuthCustomerComponent } from './tdc-customer/tdc-auth-customer/tdc-auth-customer.component';

import { DistrictAllocasionApartmentComponent } from './tdc-report-apartment/district-allocasion-apartment/district-allocasion-apartment.component';
import { DistrictAllocasionPlatformComponent } from './tdc-report-platform/district-allocasion-platform/district-allocasion-platform.component';

import { DataImportApartmentComponent } from './data-import-apartment/data-import-apartment.component';
import { DataImportPlatformComponent } from './data-import-platform/data-import-platform.component';

const COMPONENTS: Array<Type<void>> = [
  IngredientspriceComponent,
  AddOrUpdateIngredientsPriceComponent,
  OriginalPriceAndTaxComponent,
  AddOrUpdateOPATComponent,
  ResettlementApartmentComponent,
  AddOrUpdateResettlementApartmentComponent,
  ProfitValueComponent,
  AddOrUpdateProfitValueComponent,
  AnnualInstallmentComponent,
  AddOrUpdateAnnualInstallmentComponent,
  LandComponent,
  AddOrUpdateLandComponent,
  BlockHouseComponent,
  AddOrUpdateBlockHouseComponent,
  FloorTdcComponent,
  AddOrUpdateFloorTdcComponent,
  ApartmentTdcComponent,
  AddOrUpdateApartmentTdcComponent,
  TdcProjectComponent,
  AddOrUpdateProjectComponent,
  TdcProjectIngrepriceComponent,
  TdcProjectPriceAndTaxComponent,
  PlatformTdcComponent,
  AddOrUpdatePlatformTdcComponent,
  TdcCustomerComponent,
  TdcCustomerFileComponent,
  AddOrUpdateCsComponent,
  TdcReportApartmentComponent,
  AddOrUpdateApartmentReportComponent,
  TdcReportPlatformComponent,
  AddOrUpdatePlatformReportComponent,
  Report2AllComponent,
  TdcMemberComponent,
  DistrictAllocasionApartmentComponent,
  DistrictAllocasionPlatformComponent,
  TdcAuthCustomerComponent,
  DataImportApartmentComponent,
  DataImportPlatformComponent,
];

@NgModule({
  imports: [
    SharedModule,
    TDCRoutingModule,
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
export class TDCModule {}
