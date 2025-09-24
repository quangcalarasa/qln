import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { TypeAttributeComponent } from './type-attribute/type-attribute.component';
import { TemplateComponent } from './template/template.component';
import { VerifycationUnitComponent } from './verifycation-unit/verifycation-unit.component';
import { DepartmentComponent } from './department/department.component';
import { PositionComponent } from './position/position.component';

import { HolidayComponent } from './holiday/holiday.component';

import { UnitPriceComponent } from './unit-price/unit-price.component';
import { LaneComponent } from './lane/lane.component';
import { ProvinceComponent } from './province/province.component';
import { DistrictComponent } from './district/district.component';
import { WardComponent } from './ward/ward.component';
import { VatComponent } from './vat/vat.component';
import { ManualDocumentComponent } from './manual-document/manual-document.component';

const routes: Routes = [
  { path: 'type-attribute', component: TypeAttributeComponent, data: { title: 'Loại hình' } },
  { path: 'template', component: TemplateComponent, data: { title: 'Biểu mẫu' } },
  { path: 'verify-unit', component: VerifycationUnitComponent, data: { title: 'Cơ quan xác minh' } },
  { path: 'department', component: DepartmentComponent, data: { title: 'Phòng ban' } },
  { path: 'position', component: PositionComponent, data: { title: 'Chức danh' } },
  { path: 'holiday', component: HolidayComponent, data: { title: 'Kỳ nghỉ lễ' } },
  { path: 'unit-price', component: UnitPriceComponent, data: { title: 'Đơn vị tính' } },
  { path: 'lane', component: LaneComponent, data: { title: 'Đường' } },
  { path: 'province', component: ProvinceComponent, data: { title: 'Tỉnh/thành phố' } },
  { path: 'district', component: DistrictComponent, data: { title: 'Quận/huyện' } },
  { path: 'ward', component: WardComponent, data: { title: 'Phường/xã' } },
  { path: 'vat', component: VatComponent, data: { title: 'Hệ số VAT' } },
  { path: 'manual-document', component: ManualDocumentComponent, data: { title: 'Hướng dẫn sử dụng' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CategoryRoutingModule { }
