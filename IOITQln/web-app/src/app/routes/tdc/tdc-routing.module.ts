import { Component, NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { IngredientspriceComponent } from './ingredientsprice/ingredientsprice.component';
import { OriginalPriceAndTaxComponent } from './original-price-and-tax/original-price-and-tax.component';
import { ResettlementApartmentComponent } from './resettlement-apartment/resettlement-apartment.component';
import { ProfitValueComponent } from './profit-value/profit-value.component';
import { AnnualInstallmentComponent } from './annual-installment/annual-installment.component';
import { LandComponent } from './land/land.component';
import { BlockHouseComponent } from './block-house/block-house.component';
import { FloorTdcComponent } from './floor-tdc/floor-tdc.component';
import { ApartmentTdcComponent } from './apartment-tdc/apartment-tdc.component';
import { TdcProjectComponent } from './tdc-project/tdc-project.component';
import { PlatformTdcComponent } from './platform-tdc/platform-tdc.component';
import { TdcCustomerComponent } from './tdc-customer/tdc-customer.component';
import { TdcReportApartmentComponent } from './tdc-report-apartment/tdc-report-apartment.component';
import { TdcReportPlatformComponent } from './tdc-report-platform/tdc-report-platform.component';
import { Report2AllComponent } from './report2-all/report2-all.component';
import { DataImportApartmentComponent } from './data-import-apartment/data-import-apartment.component';
import { DataImportPlatformComponent } from './data-import-platform/data-import-platform.component';

const routes: Routes = [
  { path: 'ingredients-price', component: IngredientspriceComponent, data: { title: 'Thành phần giá cấu thành' } },
  { path: 'original-price-and-tax', component: OriginalPriceAndTaxComponent, data: { title: 'Thành phần giá gốc - thuế phí' } },
  { path: 'resettlement-apartment', component: ResettlementApartmentComponent, data: { title: 'Chung cư tái định cư' } },
  { path: 'profit-value', component: ProfitValueComponent, data: { title: 'Hệ số lãi phạt thuê' } },
  { path: 'annual-installment-interest-rate', component: AnnualInstallmentComponent, data: { title: 'Lãi suất trả góp hàng năm' } },
  { path: 'land', component: LandComponent, data: { title: 'Lô' } },
  { path: 'block-house', component: BlockHouseComponent, data: { title: 'Khối nhà' } },
  { path: 'floor-tdc', component: FloorTdcComponent, data: { title: 'Tầng' } },
  { path: 'apartment-tdc', component: ApartmentTdcComponent, data: { title: 'Căn' } },
  { path: 'tdc-project', component: TdcProjectComponent, data: { title: 'Dự án tái định cư' } },
  { path: 'platform-tdc', component: PlatformTdcComponent, data: { title: 'Nền đất' } },
  { path: 'customer-tdc', component: TdcCustomerComponent, data: { title: 'Khách Hàng Tái Định Cư' } },
  { path: 'report_TDC/report_Apartment', component: TdcReportApartmentComponent, data: { title: 'Danh sách căn hộ trống tiếp nhận và quản lý'}},
  { path: 'report_TDC/report_platform' , component: TdcReportPlatformComponent, data: { title: 'Danh sách nền đất trống tiếp nhận và quản lý'}},
  { path: 'report_TDC/Report2_TDC' , component: Report2AllComponent, data: { title: 'Báo cáo tổng hợp số 2'}},
  
  { path: 'data-import-tdc/data-import-tdc-apartment' , component: DataImportApartmentComponent, data: { title: 'Import dữ liệu tiếp nhận và quản lý căn hộ'}},

  { path: 'data-import-tdc/data-import-tdc-platform' , component: DataImportPlatformComponent, data: { title: 'Import dữ liệu tiếp nhận và quản lý nền đất'}},

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TDCRoutingModule { }
