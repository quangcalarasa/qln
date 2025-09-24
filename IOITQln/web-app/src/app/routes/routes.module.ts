import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';

// dashboard pages  
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './login/login.component';
import { SubSystemComponent } from './subsystem/subsystem.component';
import { RouteRoutingModule } from './routes-routing.module';
import { NgxCurrencyModule } from "ngx-currency";
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { G2PieModule } from '@delon/chart/pie';
import { G2BarModule } from '@delon/chart/bar';
import { G2MiniBarModule } from '@delon/chart/mini-bar';

const COMPONENTS: Array<Type<void>> = [
  DashboardComponent,
  LoginComponent,
  SubSystemComponent,
];

@NgModule({
  imports: [SharedModule, RouteRoutingModule, G2BarModule , G2PieModule, G2MiniBarModule, NgxCurrencyModule.forRoot(customCurrencyMaskConfig)],
  declarations: COMPONENTS,
})
export class RoutesModule { }
