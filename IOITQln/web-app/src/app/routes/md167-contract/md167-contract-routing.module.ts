import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ListContractComponent } from './list-contract/list-contract.component';

const routes: Routes = [
  { path: '', component: ListContractComponent, data: { title: 'Danh sách hợp đồng cho thuê' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class Md167ContractRoutingModule { }
