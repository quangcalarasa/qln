import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { RentBlockComponent } from './rent-block/rent-block.component';
import { RentApartmentComponent } from './rent-apartment/rent-apartment.component';
import { TimeCoefficientComponent } from './time-coefficient/time-coefficient.component';
import { RentingPriceComponent } from './RentPricing/renting-price.component';
import { ConversionComponent } from './conversion/conversion.component';
import { DefaultCoefficientComponent } from './default-coefficient/default-coefficient.component';
import { RentFileComponent } from './rent-file/rent-file.component';
import { DebtsComponent } from './debts/debts.component';
import { SalaryComponent } from './salary/salary.component';
import { DiscountCoefficientComponent } from './discount-coefficient/discount-coefficient.component';
import { QuickMathComponent } from './QuickMath/QuickMath.component';

const routes: Routes = [
  { path: 'rent-block', component: RentBlockComponent, data: { title: 'Căn nhà thuê' } },
  { path: 'rent-apartment', component: RentApartmentComponent, data: { title: 'Căn hộ thuê' } },
  { path: 'time-layout-coefficient', component: TimeCoefficientComponent, data: { title: 'Hệ số thời điểm bố trí' } },
  { path: 'renting-price', component: RentingPriceComponent, data: { title: 'Bảng giá thuê' } },
  { path: 'conversion-coefficient', component: ConversionComponent, data: { title: 'Hệ số quy đổi' } },
  { path: 'Default-Coefficient', component: DefaultCoefficientComponent, data: { title: 'Hệ số mặc định' } },
  { path: 'rent_file', component: RentFileComponent, data: { title: 'Hồ sơ thuê' } },
  { path: 'debts', component: DebtsComponent, data: { title: 'Công nợ' } },
  { path: 'salary', component: SalaryComponent, data: { title: 'Hệ số lương' } },
  { path: 'dis_cofficient', component: DiscountCoefficientComponent, data: { title: 'Hệ số giảm giá' } },
  { path: 'quick_math', component: QuickMathComponent, data: { title: 'Công cụ tính giá nhanh' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CateRentNocRoutingModule {}
