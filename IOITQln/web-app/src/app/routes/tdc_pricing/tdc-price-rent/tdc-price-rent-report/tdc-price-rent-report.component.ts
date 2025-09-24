import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';

@Component({
  selector: 'app-tdc-price-rent-report',
  templateUrl: './tdc-price-rent-report.component.html'
})
export class TdcPriceRentReportComponent implements OnInit {
  @Input() selectedItems: NzSafeAny;

  data: any[] = [];
  dataExcelTables: any[] = [];

  constructor(private tdcPriceRentRepository: TdcPriceRentRepository) {}

  ngOnInit(): void {
    this.getDataReport();
  }

  async getDataReport() {
    const resp = await this.tdcPriceRentRepository.GetReportTable(this.selectedItems);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data.concat(this.data);
    }
  }

  async exportReport() {
    const resp = await this.tdcPriceRentRepository.ExportReport(this.data);
  }


}
