import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

import { CategoryRoutingModule } from './category-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { DatePipe } from '@angular/common';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';

import { TypeAttributeComponent } from './type-attribute/type-attribute.component';
import { AddOrUpdateTypeAttributeComponent } from './type-attribute/add-or-update/add-or-update-type-attribute.component';
import { AddOrUpdateTypeAttributeItemComponent } from './type-attribute-item/add-or-update/add-or-update-type-attribute-item.component';

import { TemplateComponent } from './template/template.component';
import { AddOrUpdateTemplateComponent } from './template/add-or-update/add-or-update-template.component';

import { VerifycationUnitComponent } from './verifycation-unit/verifycation-unit.component';
import { AddOrUpdateVerifycationUnitComponent } from './verifycation-unit/add-or-update/add-or-update-verifycation-unit.component';

import { DepartmentComponent } from './department/department.component';
import { AddOrUpdateDepartmentComponent } from './department/add-or-update/add-or-update-department.component';

import { PositionComponent } from './position/position.component';
import { AddOrUpdatePositionComponent } from './position/add-or-update/add-or-update-position.component';

import { HolidayComponent } from './holiday/holiday.component';
import { AddOrUpdateHolidayComponent } from './holiday/add-or-update/add-or-update-holiday.component';
import { CalendarComponent } from './holiday/calendar/calendar.component';
import { AgGridComponent } from './holiday/ag-grid/ag-grid.component';

import { UnitPriceComponent } from './unit-price/unit-price.component';
import { AddOrUpdateUnitPriceComponent } from './unit-price/add-or-update/add-or-update-unit-price.component';

import { LaneComponent } from './lane/lane.component';
import { AddOrUpdateLaneComponent } from './lane/add-or-update/add-or-update-lane.component';

import { ProvinceComponent } from './province/province.component';
import { AddOrUpdateProvinceComponent } from './province/add-or-update/add-or-update-province.component';

import { DistrictComponent } from './district/district.component';
import { AddOrUpdateDistrictComponent } from './district/add-or-update/add-or-update-district.component';

import { WardComponent } from './ward/ward.component';
import { AddOrUpdateWardComponent } from './ward/add-or-update/add-or-update-ward.component';

import { VatComponent } from './vat/vat.component';
import { AddOrUpdateVatComponent } from './vat/add-or-update/add-or-update-vat.component';

import { ManualDocumentComponent } from './manual-document/manual-document.component';
import { AddOrUpdateManualDocumentComponent } from './manual-document/add-or-update/add-or-update-manual-document.component';

const COMPONENTS: Array<Type<void>> = [
  TypeAttributeComponent,
  AddOrUpdateTypeAttributeComponent,
  AddOrUpdateTypeAttributeItemComponent,
  TemplateComponent,
  AddOrUpdateTemplateComponent,
  VerifycationUnitComponent,
  AddOrUpdateVerifycationUnitComponent,
  DepartmentComponent,
  AddOrUpdateDepartmentComponent,
  PositionComponent,
  AddOrUpdatePositionComponent,
  HolidayComponent,
  AddOrUpdateHolidayComponent,
  CalendarComponent,
  AgGridComponent,
  UnitPriceComponent,
  AddOrUpdateUnitPriceComponent,
  LaneComponent,
  AddOrUpdateLaneComponent,
  ProvinceComponent,
  AddOrUpdateProvinceComponent,
  DistrictComponent,
  AddOrUpdateDistrictComponent,
  WardComponent,
  AddOrUpdateWardComponent,
  VatComponent,
  AddOrUpdateVatComponent,
  ManualDocumentComponent,
  AddOrUpdateManualDocumentComponent
];

@NgModule({
  imports: [
    SharedModule,
    CategoryRoutingModule,
    NzPageHeaderModule,
    NzTreeSelectModule,
    NzUploadModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
    NzCalendarModule,
    PipeModule
  ],
  declarations: COMPONENTS,
  providers: [DatePipe]
})
export class CategoryModule { }
