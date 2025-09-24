import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CateRentNocRoutingModule } from './cate-rent-noc-routing.module';
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

import { RentBlockComponent } from './rent-block/rent-block.component';
import { AddOrUpdateRentBlockComponent } from './rent-block/add-or-update/add-or-update-rent-block.component';
import { RentBlockDetailComponent } from './rent-block/rent-block-detail/rent-block-detail.component';

import { RentApartmentComponent } from './rent-apartment/rent-apartment.component';
import { AddOrUpdateRentApartmentComponent } from './rent-apartment/add-or-update/add-or-update-rent-apartment.component';

import { TimeCoefficientComponent } from './time-coefficient/time-coefficient.component';
import { AddOrUpdateTimeCoefficientComponent } from './time-coefficient/add-or-update-time-coefficient/add-or-update-time-coefficient.component';

import { RentingPriceComponent } from './RentPricing/renting-price.component';
import { AddOrUpdateRentingPriceComponent } from './RentPricing/add-or-update/add-or-update-renting-price.component';

import { ConversionComponent } from './conversion/conversion.component';
import { AddOrUpdateConversionComponent } from './conversion/add-or-update-conversion/add-or-update-conversion.component';

import { DefaultCoefficientComponent } from './default-coefficient/default-coefficient.component';
import { AddOrUpdateDefaultCoefficientComponent } from './default-coefficient/add-or-update-default-coefficient/add-or-update-default-coefficient.component';

import { RentFileComponent } from './rent-file/rent-file.component';
import { AddOrUpdateRentFileComponent } from './rent-file/add-or-update-rent-file/add-or-update-rent-file.component';
import { MemberRentFileComponent } from './rent-file/member-rent-file/member-rent-file.component';

import { RentTableComponent } from './rent-file/rent-table/rent-table.component';
import { DebtComponent } from './rent-file/debt/debt.component';
import { PaymentComponent } from './rent-file/payment/payment.component';
import { ReceiptsComponent } from './rent-file/receipts/receipts.component';

import { DebtsComponent } from './debts/debts.component';

import { AddendumComponent } from './rent-file/addendum/addendum.component';
import { ListAddendumComponent } from './rent-file/list-addendum/list-addendum.component';
import { ImportDebtComponent } from './rent-file/import-debt/import-debt.component';
import { ArrearsComponent } from './rent-file/arrears/arrears.component';
import { ListArearsComponent } from './rent-file/list-arears/list-arears.component';
import { BctForUserComponent } from './rent-file/bct-for-user/bct-for-user.component';

import { SalaryComponent } from './salary/salary.component';
import { AddOrUpdateSalaryComponent } from './salary/add-or-update-salary/add-or-update-salary.component';

import { DiscountCoefficientComponent } from './discount-coefficient/discount-coefficient.component';
import { AddOrUpdateDiscountComponent } from './discount-coefficient/add-or-update-discount/add-or-update-discount.component';

import { RentBlockAddressComponent } from 'src/app/routes/cate-rent-noc/rent-block/rent-block-address/rent-block-address.component';

import { ExportPromissoryComponent } from 'src/app/routes/cate-rent-noc/rent-file/export-promissory/export-promissory.component';
import { NocRentBlockFileComponent } from './rent-block/noc-rent-block-file/noc-rent-block-file.component';
import { NocRentApartmentFileComponent } from './rent-apartment/noc-rent-apartment-file/noc-rent-apartment-file.component';
import { QuickMathComponent } from './QuickMath/QuickMath.component';
import { AddQuickMathComponent } from './QuickMath/AddQiuckMath/AddQuickMath.component';
import { LogsQuickMathComponent } from './QuickMath/LogsQuickMath/LogsQuickMath.component';
const COMPONENTS: Array<Type<void>> = [
  RentBlockComponent,
  AddOrUpdateRentBlockComponent,
  RentBlockDetailComponent,
  RentApartmentComponent,
  AddOrUpdateRentApartmentComponent,
  TimeCoefficientComponent,
  AddOrUpdateTimeCoefficientComponent,
  RentingPriceComponent,
  AddOrUpdateRentingPriceComponent,
  ConversionComponent,
  AddOrUpdateConversionComponent,
  DefaultCoefficientComponent,
  AddOrUpdateDefaultCoefficientComponent,
  RentFileComponent,
  AddOrUpdateRentFileComponent,
  MemberRentFileComponent,
  RentTableComponent,
  DebtComponent,
  PaymentComponent,
  ReceiptsComponent,
  DebtsComponent,
  AddendumComponent,
  ListAddendumComponent,
  ImportDebtComponent,
  ArrearsComponent,
  ListArearsComponent,
  BctForUserComponent,
  SalaryComponent,
  AddOrUpdateSalaryComponent,
  DiscountCoefficientComponent,
  AddOrUpdateDiscountComponent,
  RentBlockAddressComponent,
  ExportPromissoryComponent,
  NocRentBlockFileComponent,
  NocRentApartmentFileComponent,
  QuickMathComponent,
  AddQuickMathComponent,
  LogsQuickMathComponent
];

@NgModule({
  imports: [
    SharedModule,
    CateRentNocRoutingModule,
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
export class CateRentNocModule {}
