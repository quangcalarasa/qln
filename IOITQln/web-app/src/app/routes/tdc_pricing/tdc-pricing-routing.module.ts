import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TdcPriceRentComponent } from './tdc-price-rent/tdc-price-rent.component';
import { TdcInstallmentPriceComponent } from './tdc-installment-price/tdc-installment-price.component';
import { TdcPriceOneSellComponent } from './tdc-price-one-sell/tdc-price-one-sell.component';


const routes: Routes = [
  { path: 'priceRent-tdc', component: TdcPriceRentComponent, data: { title: 'Tính giá thuê' } },
  { path: 'Installment-price', component: TdcInstallmentPriceComponent, data: { title: 'Tính Giá Trả Góp' } },
  { path: 'price-one-time-tdc', component: TdcPriceOneSellComponent, data: { title: 'Tính Giá Bán Một Lần' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TdcPricingRoutingModule { }
