import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FunctionComponent } from './function/function.component';
import { RoleComponent } from './role/role.component';
import { UserComponent } from './user/user.component';
import { LogActionComponent } from './log-action/log-action.component';

const routes: Routes = [
  { path: 'function', component: FunctionComponent, data: { title: 'Chức năng hệ thống' } },
  { path: 'role', component: RoleComponent, data: { title: 'Nhóm quyền hệ thống' } },
  { path: 'user', component: UserComponent, data: { title: 'Tài khoản hệ thống' } },
  { path: 'log-action', component: LogActionComponent, data: { title: 'Nhật ký hệ thống' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemRoutingModule { }
