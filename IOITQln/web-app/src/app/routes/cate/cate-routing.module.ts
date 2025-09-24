import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DecreeComponent } from './decree/decree.component';
import { LandPriceComponent } from './land-price/land-price.component';
import { CustomerComponent } from './customer/customer.component';

const routes: Routes = [
  { path: 'decree/decree-type1', component: DecreeComponent, data: { title: 'Nghị định' } },
  { path: 'decree/decree-type2', component: DecreeComponent, data: { title: 'Văn bản pháp luật liên quan' } },
  { path: 'land-price', component: LandPriceComponent, data: { title: 'Bảng giá đất' } },
  { path: 'customer', component: CustomerComponent, data: { title: 'Khách hàng' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CateRoutingModule {}
