import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { DefaultCoefficientRepository } from '../../../../infrastructure/repositories/default-coefficient.repositories';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { SettingsService } from '@delon/theme';
import { PromissoryRepository } from '../../../../infrastructure/repositories/Promissory.repository';
import { NzModalService, NzModalRef } from 'ng-zorro-antd/modal';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { TdcPriceRentRepository } from '../../../../infrastructure/repositories/tdcPriceRent.repository';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { TdcPriceOneSellRepository } from 'src/app/infrastructure/repositories/tdcPriceOneSell.repository';

@Component({
  selector: 'app-show-log-temporary',
  templateUrl: './show-log-temporary.component.html',
  styles: []
})
export class ShowLogTemporaryComponent implements OnInit {
  @Input() Id: NzSafeAny;
  @Input() type: NzSafeAny;


  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data: any;

  constructor(
    private tdcPriceRentRepository: TdcPriceRentRepository,
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
    private tdcPriceOneSellRepository: TdcPriceOneSellRepository,
    private fb: FormBuilder,
    private message: NzMessageService,
    private modal: NzModalRef,
    private settings: SettingsService
  ) {}
  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'IngredientsPriceId' },
    { renderTitle: 'AreaHeader', render: 'Area' },
    { renderTitle: 'PriceHeader', render: 'Price' },
    { renderTitle: 'TotalHeader', render: 'Total' }
  ];
  ngOnInit(): void {
    this.getData();
  }

  async getData() {
    if(this.type==1)
    {
      const resp = await this.tdcPriceRentRepository.getLog(this.Id);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      }
    }
    else if(this.type==2)
    {
      const resp = await this.tdcPriceOneSellRepository.getLog(this.Id);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      }
    }
    else if(this.type==3)
    {
      const resp = await this.tDCInstallmentPriceRepository.getLog(this.Id);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      }
    }
  }
}
