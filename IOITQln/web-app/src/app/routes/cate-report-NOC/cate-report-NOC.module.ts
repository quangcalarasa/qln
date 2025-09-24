import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CateReportNOCRoutingModule } from './cate-report-NOC-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';

import { ReportNOC5Component } from './report-noc5/report-noc5.component';
import { ReportNoc4Component } from './report-noc4/report-noc4.component';
import { ReportNoc3Component } from './report-noc3/report-noc3.component';
import { ReportNoc6Component } from './report-noc6/report-noc6.component';
import { ReportNoc7Component } from './report-noc7/report-noc7.component';
import { ReportNOC1Component } from './report-noc1/report-noc1.component';
import { ReportNoc2Component } from './report-noc2/report-noc2.component';
import { SyntheticReportComponent } from './synthetic/synthetic.component';
import { SyntheticItemComponent } from './synthetic-item/synthetic-item.component';
import { CustomerReportComponent } from './customer/customer.component';
import { UsageStatusReportComponent } from './usage-status/usage-status.component';
import { DebtContractReportComponent } from './debt-contract/debt-contract.component';
import { DueContractReportComponent } from './due-contract/due-contract.component';
import { OverdueContractReportComponent } from './overdue-contract/overdue-contract.component';

const COMPONENTS: Array<Type<void>> = [
  ReportNOC5Component,
  ReportNoc4Component,
  ReportNoc3Component,
  ReportNoc6Component,
  ReportNoc7Component,
  ReportNOC1Component,
  ReportNoc2Component,
  SyntheticReportComponent,
  SyntheticItemComponent,
  CustomerReportComponent,
  UsageStatusReportComponent,
  DebtContractReportComponent,
  DueContractReportComponent,
  OverdueContractReportComponent
];

@NgModule({
  imports: [
    SharedModule,
    CateReportNOCRoutingModule,
    NzPageHeaderModule,
    NzTreeSelectModule,
    NzUploadModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
    NzCalendarModule
  ],
  declarations: COMPONENTS,
  providers: [DatePipe]
})
export class CateReportNOCModule {}
