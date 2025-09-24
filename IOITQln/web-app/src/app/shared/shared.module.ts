import { NgModule, Type } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AlainThemeModule } from '@delon/theme';
import { DelonACLModule } from '@delon/acl';
import { DelonFormModule } from '@delon/form';

import { SHARED_DELON_MODULES } from './shared-delon.module';
import { SHARED_ZORRO_MODULES } from './shared-zorro.module';
import { DeleteCellRenderer } from './custom_editor/delete-cell-renderer';
import { AgGridModule } from 'ag-grid-angular';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';

//Directive
import { DebounceClickDirective } from 'src/app/shared/directive/debounce-click.directive';

// eslint-disable-next-line import/no-unassigned-import
import 'ag-grid-enterprise';
import { LicenseManager } from 'ag-grid-enterprise';

import { SelectAgGrid } from './custom_editor/select-ag-grid';
import { DetailImportEximComponent } from './components/exims/detail-import.component';
import { AutocompleAgGrid } from './custom_editor/autocomplete/autocomplete-editor';
import { CheckboxEditor } from './custom_editor/checkbox.component';
import { PDFViewerComponent } from './components/pdf-viewer/pdf-viewer.component';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';
import { SharedPopoverComponent } from './components/popover/popover.component';
import { LandscapePositionInfoComponent } from './components/landscape-position-info/landscape-position-info.component';
import { LandscapePositionComponent } from './components/landscape-position/landscape-position.component';
import { BlockLandSpecialComponent } from './components/land-special/land-special.component';
import { AdjacentLandSharedComponent } from './components/adjacent-land/adjacent-land.component';
import { SharedImportExcelComponent } from './components/import-excel/import-excel.component';
import { SharedConfirmUpdateMdComponent } from './components/confirm-update-md/confirm-update-md.component';
import { SharedConfirmUpdateListComponent } from './components/confirm-update-list/confirm-update-list.component';
import { SharedTreeViewCheckboxComponent } from './components/tree-view-checkbox/tree-view-checkbox.component';
import { SharedAgGridLoadingOverlayComponent } from './components/ag-grid/loading-overlay';

// #region third libs

const THIRDMODULES: Array<Type<void>> = [AgGridModule];

// #endregion

// #region your componets & directives

const COMPONENTS: Array<Type<void>> = [
  DeleteCellRenderer,
  SelectAgGrid,
  AutocompleAgGrid,
  DetailImportEximComponent,
  CheckboxEditor,
  PDFViewerComponent,
  SharedPopoverComponent,
  LandscapePositionInfoComponent,
  LandscapePositionComponent,
  BlockLandSpecialComponent,
  AdjacentLandSharedComponent,
  SharedImportExcelComponent,
  SharedConfirmUpdateMdComponent,
  SharedConfirmUpdateListComponent,
  SharedTreeViewCheckboxComponent,
  SharedAgGridLoadingOverlayComponent
];
const DIRECTIVES: Array<Type<void>> = [];

// #endregion

const licenseKey =
  'CompanyName=Equinix Asia Pacific pte ltd,LicensedGroup=equinixMendixPrivateLib,LicenseType=MultipleApplications,LicensedConcurrentDeveloperCount=2,LicensedProductionInstancesCount=0,AssetReference=AG-027567,SupportServicesEnd=18_June_2023_[v2]_MTY4NzA0MjgwMDAwMA==4be2c388f9a8a7443c72842dff53d5b2';
LicenseManager.setLicenseKey(licenseKey);

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    ReactiveFormsModule,
    AlainThemeModule.forChild(),
    DelonACLModule,
    DelonFormModule,
    ...SHARED_DELON_MODULES,
    ...SHARED_ZORRO_MODULES,
    // third libs
    ...THIRDMODULES,
    PipeModule,
    NgxCurrencyModule.forRoot(customCurrencyMaskConfig)
  ],
  declarations: [
    // your components
    ...COMPONENTS,
    ...DIRECTIVES,
    DebounceClickDirective
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    AlainThemeModule,
    DelonACLModule,
    DelonFormModule,
    ...SHARED_DELON_MODULES,
    ...SHARED_ZORRO_MODULES,
    // third libs
    ...THIRDMODULES,
    // your components
    ...COMPONENTS,
    ...DIRECTIVES,
    DebounceClickDirective
  ]
})
export class SharedModule { }
