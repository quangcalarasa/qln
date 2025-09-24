import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DataImportNocComponent } from './noc/noc.component';

const routes: Routes = [
  { path: 'data-import-noc', component: DataImportNocComponent, data: { title: 'Import dữ liệu tổng hợp nhà ở cũ' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DataImportRoutingModule { }
