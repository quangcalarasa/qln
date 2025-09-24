import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { SystemRoutingModule } from './system-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';

import { FunctionComponent } from './function/function.component';
import { AddOrUpdateFunctionComponent } from './function/add-or-update/add-or-update-function.component';

import { RoleComponent } from './role/role.component';
import { AddOrUpdateRoleComponent } from './role/add-or-update/add-or-update-role.component';

import { UserComponent } from './user/user.component';
import { AddOrUpdateUserComponent } from './user/add-or-update/add-or-update-user.component';
import { ChangePassUserComponent } from './user/change-pass/change-pass-user.component';

import { LogActionComponent } from './log-action/log-action.component';

const COMPONENTS: Array<Type<void>> = [
  FunctionComponent,
  AddOrUpdateFunctionComponent,
  RoleComponent,
  AddOrUpdateRoleComponent,
  UserComponent,
  AddOrUpdateUserComponent,
  ChangePassUserComponent,
  LogActionComponent
];

@NgModule({
  imports: [SharedModule, SystemRoutingModule, NzPageHeaderModule, NzTreeSelectModule, NzUploadModule],
  declarations: COMPONENTS
})
export class SystemModule { }
