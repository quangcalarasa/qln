import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { TdcPriceOneSellRepository } from 'src/app/infrastructure/repositories/tdcPriceOneSell.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';


@Component({
  selector: 'app-tdc-price-one-sell-table',
  templateUrl: './tdc-price-one-sell-table.component.html'
})
export class TdcPriceOneSellTableComponent implements OnInit {

  @Input() input: NzSafeAny;
  data: any[] = [];
  constructor(private tdcPriceOneSellRepository: TdcPriceOneSellRepository) { }


  ngOnInit(): void {
    this.getDataTable();
  }

  async getDataTable(){
    const reps = await this.tdcPriceOneSellRepository.GetReportTable(this.input);
    if(reps.meta?.error_code==200){
      this.data = reps.data;
    }
  }
  async getExportEx(){
    const resp = await this.tdcPriceOneSellRepository.getExportExcel(this.data);
  }

}
