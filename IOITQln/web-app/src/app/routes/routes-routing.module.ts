import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SimpleGuard } from '@delon/auth';
import { environment } from '@env/environment';
import { AuthGuard } from 'src/app/core/auth/auth.guard';

// layout
import { LayoutBasicComponent } from '../layout/basic/basic.component';
import { LayoutLoginComponent } from '../layout/login/login.component';
// dashboard pages
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './login/login.component';
import { SubSystemComponent } from './subsystem/subsystem.component';
import { SupportComponent } from './extra-info/Contact/Support.component';
import { ContactManageComponent } from './extra-info/Contact-Management/ContactManageComponent';


const routes: Routes = [
  {
    path: '',
    component: LayoutBasicComponent,
    canActivate: [SimpleGuard, AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent, data: { title: 'Tổng quan' } },
      { path: 'category', loadChildren: () => import('./category/category.module').then(m => m.CategoryModule) },
      { path: 'system', loadChildren: () => import('./system/system.module').then(m => m.SystemModule) },
      { path: 'account', loadChildren: () => import('./account/account.module').then(m => m.AccountModule) },
      // { path: 'cate-common', loadChildren: () => import('./cate-common/cate-common.module').then(m => m.CateCommonModule) },
      { path: 'cate', loadChildren: () => import('./cate/cate.module').then(m => m.CateModule) },
      { path: 'cate-tdc', loadChildren: () => import('./tdc/tdc.module').then(m => m.TDCModule) },
      { path: 'tdcPricing', loadChildren: () => import('./tdc_pricing/pricing.module').then(m => m.TdcPricingModule) },
      { path: 'cate-md167', loadChildren: () => import('./cate-md167/cate-md167.module').then(m => m.CateMd167Module) },
      { path: 'md167-contract', loadChildren: () => import('./md167-contract/md167-contract.module').then(m => m.Md167ContractModule) },
      { path: 'report_NOC', loadChildren: () => import('./cate-report-NOC/cate-report-NOC.module').then(m => m.CateReportNOCModule) },
      { path: '', loadChildren: () => import('./report-md167/report-md167.module').then(m => m.ReportMd167Module) },
      { path: 'extra-info', loadChildren: () => import('./extra-info/extra-info.module').then(m => m.ExtraInfoModule) },
      { path: 'AddContact', component: SupportComponent, data: { title: 'Gửi phán anh kiến nghị', breadcrumb: 'Gửi phán anh kiến nghị' } },
      { path: 'ContactManagement', component: ContactManageComponent , data: { title: 'Phản ánh kiến nghị', breadcrumb: 'Phản ánh kiến nghị' } },
      { path: '', loadChildren: () => import('./data-import/data-import.module').then(m => m.DataImportModule) },
      { path: 'rent-cost-info', loadChildren: () => import('./cate-rent-noc/cate-rent-noc.module').then(m => m.CateRentNocModule) },
      { path: 'sell-cost-info', loadChildren: () => import('./cate-sell-noc/cate-sell-noc.module').then(m => m.CateSellNocModule) },
    ]
  },
  {
    path: '',
    component: LayoutLoginComponent,
    children: [
      { path: 'login', component: LoginComponent, data: { title: 'Đăng nhập', breadcrumb: 'Đăng nhập' } },
      {
        path: 'subsystem',
        component: SubSystemComponent,
        data: { title: 'Phân hệ chức năng', breadcrumb: 'Phân hệ chức năng' },
        canActivate: [SimpleGuard]
      },

    ]
  },
  {
    path: '',
    component: LayoutBasicComponent,
    // canActivate: [SimpleGuard],
    children: [{ path: 'exception', loadChildren: () => import('./exception/exception.module').then(m => m.ExceptionModule) }]
  },
  { path: '**', redirectTo: 'exception/404' }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      useHash: environment.useHash,
      // NOTICE: If you use `reuse-tab` component and turn on keepingScroll you can set to `disabled`
      // Pls refer to https://ng-alain.com/components/reuse-tab
      scrollPositionRestoration: 'top'
    })
  ],
  exports: [RouterModule]
})
export class RouteRoutingModule { }
