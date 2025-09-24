import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AccountSettingsComponent } from './account.component';
import { ChangeInfoComponent } from './change-info/change-info.component';
import { ChangePassComponent } from './change-pass/change-pass.component';

const routes: Routes = [
  {
    path: '',
    component: AccountSettingsComponent,
    children: [
      {
        path: 'changeinfo',
        component: ChangeInfoComponent,
        data: { title: 'Thay đổi thông tin tài khoản' }
      },
      {
        path: 'changepass',
        component: ChangePassComponent,
        data: { title: 'Thay đổi mật khẩu' }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountRoutingModule {}
