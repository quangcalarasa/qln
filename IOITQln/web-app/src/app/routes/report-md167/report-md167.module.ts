import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { ReportMd167RoutingModule } from './report-md167-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';
import { Report07Component } from './report07/report07.component';
import { Report08Component } from './report08/report08.component';
import { DebtInfoComponent } from './debt-info/debt-info.component';
import { ReportPaymentComponent } from './report-payment/report-payment.component';



const COMPONENTS: Array<Type<void>> = [
  Report07Component,
  Report08Component,
  DebtInfoComponent,
  ReportPaymentComponent
];

@NgModule({
  imports: [
    SharedModule,
    ReportMd167RoutingModule,
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
export class ReportMd167Module { }
