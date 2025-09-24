import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { TdcPricingRoutingModule } from './tdc-pricing-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';

import { TdcPriceRentComponent } from './tdc-price-rent/tdc-price-rent.component';
import { AddOrUpdatePriceRentComponent } from './tdc-price-rent/add-or-update-price-rent/add-or-update-price-rent.component';
import { TdcPriceRentTemporaryComponent } from './tdc-price-rent/tdc-price-rent-temporary/tdc-price-rent-temporary.component';
import { TdcPriceRentOfficialComponent } from './tdc-price-rent/tdc-price-rent-official/tdc-price-rent-official.component';
import { TdcPriceRentTaxComponent } from './tdc-price-rent/tdc-price-rent-tax/tdc-price-rent-tax.component';
import { TdcInstallmentOfficalComponent } from './tdc-installment-price/tdc-installment-offical/tdc-installment-offical.component';
import { TdcInstallmentPriceAndTaxComponent } from './tdc-installment-price/tdc-installment-price-and-tax/tdc-installment-price-and-tax.component';
import { TdcInstallmentPriceComponent } from './tdc-installment-price/tdc-installment-price.component';
import { TdcInstallmentTemporaryComponent } from './tdc-installment-price/tdc-installment-temporary/tdc-installment-temporary.component';
import { AddOrUpdateInstallmentComponent } from './tdc-installment-price/add-or-update/add-or-update-installment.component';

import { TdcPriceOneSellComponent } from './tdc-price-one-sell/tdc-price-one-sell.component';
import { AddOrUpdateTdcPriceOneSellComponent } from './tdc-price-one-sell/add-or-update-tdc-price-one-sell/add-or-update-tdc-price-one-sell.component';
import { TdcPriceOneSellOfficialComponent } from './tdc-price-one-sell/tdc-price-one-sell-official/tdc-price-one-sell-official.component';
import { TdcPriceOneSellTemporaryComponent } from './tdc-price-one-sell/tdc-price-one-sell-temporary/tdc-price-one-sell-temporary.component';
import { TdcPriceOneSellTaxComponent } from './tdc-price-one-sell/tdc-price-one-sell-tax/tdc-price-one-sell-tax.component';
import { TdcPriceOneSellTableComponent } from './tdc-price-one-sell/tdc-price-one-sell-table/tdc-price-one-sell-table.component';

import { TdcPriceRentTableComponent } from './tdc-price-rent/tdc-price-rent-table/tdc-price-rent-table.component';
import { PayTdcRentComponentComponent } from './tdc-price-rent/pay-tdc-rent-component/pay-tdc-rent-component.component';
import { TdcPriceRentTableCloneComponent } from './tdc-price-rent/tdc-price-rent-table-clone/tdc-price-rent-table-clone.component';
import { PayTdcInstallmentPriceComponent } from './tdc-installment-price/pay-tdc-installment-price/pay-tdc-installment-price.component';
import { TdcInstallmentPriceTableComponent } from './tdc-installment-price/tdc-installment-price-table/tdc-installment-price-table.component';
import { ImportExcelComponent } from './tdc-installment-price/import-excel/import-excel.component';
import { TdcPriceRentImportExComponent } from './tdc-price-rent/tdc-price-rent-import-ex/tdc-price-rent-import-ex.component';
import { TdcPriceRentReportComponent } from './tdc-price-rent/tdc-price-rent-report/tdc-price-rent-report.component';
import { TdcInstallmentReportComponent } from './tdc-installment-price/tdc-installment-price-report/tdc-installment-report.component';

import { ShowLogTemporaryComponent } from './tdc-price-rent/show-log-temporary/show-log-temporary.component';
import { AddendumTDCComponent } from './tdc-installment-price/addendum-tdc/addendum-tdc.component';
import { ListAddendumTDCComponent } from './tdc-installment-price/list-addendum-tdc/list-addendum-tdc.component';

import { ShowLogOfficialComponent } from './tdc-price-one-sell/show-log-official/show-log-official.component';

const COMPONENTS: Array<Type<void>> = [
  TdcPriceRentComponent,
  AddOrUpdatePriceRentComponent,
  TdcPriceRentTemporaryComponent,
  TdcPriceRentOfficialComponent,
  TdcPriceRentTaxComponent,
  TdcInstallmentOfficalComponent,
  TdcInstallmentPriceAndTaxComponent,
  TdcInstallmentPriceComponent,
  TdcInstallmentTemporaryComponent,
  AddOrUpdateInstallmentComponent,
  TdcPriceOneSellComponent,
  AddOrUpdateTdcPriceOneSellComponent,
  TdcPriceOneSellOfficialComponent,
  TdcPriceOneSellTemporaryComponent,
  TdcPriceOneSellTaxComponent,
  TdcPriceRentTableComponent,
  PayTdcRentComponentComponent,
  TdcPriceRentTableCloneComponent,
  PayTdcInstallmentPriceComponent,
  TdcInstallmentPriceTableComponent,
  ImportExcelComponent,
  TdcPriceRentImportExComponent,
  TdcPriceRentReportComponent,
  TdcInstallmentReportComponent,
  TdcPriceOneSellTableComponent,
  ShowLogTemporaryComponent,
  AddendumTDCComponent,
  ListAddendumTDCComponent,
  ShowLogOfficialComponent
];
@NgModule({
  imports: [
    SharedModule,
    TdcPricingRoutingModule,
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
export class TdcPricingModule {}
