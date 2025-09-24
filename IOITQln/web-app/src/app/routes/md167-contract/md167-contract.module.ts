import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { Md167ContractRoutingModule } from './md167-contract-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { NgxCurrencyModule } from 'ngx-currency';
import { customCurrencyMaskConfig } from 'src/app/shared/utils/consts';
import { PipeModule } from 'src/app/shared/pipe/pipe.module';

import { ListContractComponent } from './list-contract/list-contract.component';
import { AddOrUpdateMd167ContractComponent } from './add-or-update/add-or-update-contract.component';
import { PricePerMonthComponent } from './price-per-month/price-per-month.component';
import { ValuationComponent } from './valuation/valuation.component';
import { AuctionDecisionComponent } from './auction-decision/auction-decision.component';
import { AddOrUpdateMd167ContractExtraComponent } from './add-or-update-extra/add-or-update-contract-extra.component';
import { Md167ContractDebtTableComponent } from './debt-table/debt-table.component';
import { AddOrUpdateMd167PaymentComponent } from './payment/payment.component';
import { Md167ReceiptComponent } from './receipt/receipt.component';
import { AddOrUpdateReceiptComponent } from './add-or-update-receipt/add-or-update-receipt.component';

const COMPONENTS: Array<Type<void>> = [
  ListContractComponent,
  AddOrUpdateMd167ContractComponent,
  PricePerMonthComponent,
  ValuationComponent,
  AuctionDecisionComponent,
  AddOrUpdateMd167ContractExtraComponent,
  Md167ContractDebtTableComponent,
  AddOrUpdateMd167PaymentComponent,
  Md167ReceiptComponent,
  AddOrUpdateReceiptComponent
];

@NgModule({
  imports: [SharedModule, Md167ContractRoutingModule, NzPageHeaderModule, NzTreeSelectModule, NzUploadModule, NgxCurrencyModule.forRoot(customCurrencyMaskConfig), PipeModule],
  declarations: COMPONENTS
})
export class Md167ContractModule { }
