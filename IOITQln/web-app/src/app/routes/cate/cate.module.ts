import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CateRoutingModule } from './cate-routing.module';
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

import { DecreeComponent } from './decree/decree.component';
import { AddOrUpdateDecreeComponent } from './decree/add-or-update/add-or-update-decree.component';

import { LandPriceComponent } from './land-price/land-price.component';
import { AddOrUpdateLandPriceComponent } from './land-price/add-or-update/add-or-update-land-price.component';

import { CustomerComponent } from './customer/customer.component';
import { AddOrUpdateCustomerComponent } from './customer/add-or-update/add-or-update-customer.component';

const COMPONENTS: Array<Type<void>> = [
  DecreeComponent,
  AddOrUpdateDecreeComponent,
  LandPriceComponent,
  AddOrUpdateLandPriceComponent,
  CustomerComponent,
  AddOrUpdateCustomerComponent,
];

@NgModule({
  imports: [
    SharedModule,
    CateRoutingModule,
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
export class CateModule {}
