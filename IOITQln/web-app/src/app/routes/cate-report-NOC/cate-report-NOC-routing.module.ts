import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ReportNOC5Component } from './report-noc5/report-noc5.component';
import { ReportNoc4Component } from './report-noc4/report-noc4.component';
import { ReportNoc3Component } from './report-noc3/report-noc3.component';
import { ReportNoc6Component } from './report-noc6/report-noc6.component';
import { ReportNoc7Component } from './report-noc7/report-noc7.component';
import { ReportNOC1Component } from './report-noc1/report-noc1.component';
import { ReportNoc2Component } from './report-noc2/report-noc2.component';
import { SyntheticReportComponent } from './synthetic/synthetic.component';
import { CustomerReportComponent } from './customer/customer.component';
import { UsageStatusReportComponent } from './usage-status/usage-status.component';
import { DebtContractReportComponent } from './debt-contract/debt-contract.component';
import { DueContractReportComponent } from './due-contract/due-contract.component';
import { OverdueContractReportComponent } from './overdue-contract/overdue-contract.component';

const routes: Routes = [
  { path: 'report5', component: ReportNOC5Component, data: { title: 'Báo cáo phụ lục 5' } },
  { path: 'report4', component: ReportNoc4Component, data: { title: 'Báo cáo phụ lục 4' } },
  { path: 'report3', component: ReportNoc3Component, data: { title: 'Báo cáo phụ lục 3' } },
  { path: 'report6', component: ReportNoc6Component, data: { title: 'Báo cáo phụ lục 6' } },
  { path: 'report7', component: ReportNoc7Component, data: { title: 'Báo cáo phụ lục 7' } },
  { path: 'report2', component: ReportNoc2Component, data: { title: 'Báo cáo phụ lục 2' } },
  { path: 'report1', component: ReportNOC1Component, data: { title: 'Báo cáo phụ lục 1' } },
  { path: 'synthetic', component: SyntheticReportComponent, data: { title: 'Báo cáo tổng hợp' } },
  { path: 'customer', component: CustomerReportComponent, data: { title: 'Báo cáo khách hàng' } },
  { path: 'usagestatus', component: UsageStatusReportComponent, data: { title: 'Báo cáo tình trạng sử dụng nhà' } },
  { path: 'debt-contract', component: DebtContractReportComponent, data: { title: 'Báo cáo tổng hợp công nợ khách hàng' } },
  { path: 'due-contract', component: DueContractReportComponent, data: { title: 'Báo cáo hợp đồng sắp đến hạn nộp' } },
  { path: 'overdue-contract', component: OverdueContractReportComponent, data: { title: 'Báo cáo hợp đồng nợ quá hạn' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CateReportNOCRoutingModule {}
