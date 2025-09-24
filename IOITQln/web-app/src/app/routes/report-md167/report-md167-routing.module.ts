import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DebtInfoComponent } from './debt-info/debt-info.component';
import { Report07Component } from './report07/report07.component';
import { Report08Component } from './report08/report08.component';
import { ReportPaymentComponent } from './report-payment/report-payment.component';


const routes: Routes = [
  // { path: 'type-attribute', component: TypeAttributeComponent, data: { title: 'Loại hình' } },
  { path: 'cate-md167/report-md167/report08-md167', component: Report08Component, data: { title: 'Loại hình' } },
  { path: 'cate-md167/report-md167/report07-md167', component: Report07Component, data: { title: 'Loại hình' } },
  { path: 'cate-md167/report-md167/info-debt-md167', component: DebtInfoComponent, data: { title: 'Loại hình' } },
  { path: 'cate-md167/report-md167/report-payment', component: ReportPaymentComponent, data: { title: 'Báo cáo phiếu chi' } },
  // { path: 'type-attribute', component: TypeAttributeComponent, data: { title: 'Loại hình' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportMd167RoutingModule {

}
