import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PricingComponent } from './list/pricing.component';

const routes: Routes = [
  { path: '', component: PricingComponent, data: { title: 'Biên bản tính giá' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PricingRoutingModule { }
