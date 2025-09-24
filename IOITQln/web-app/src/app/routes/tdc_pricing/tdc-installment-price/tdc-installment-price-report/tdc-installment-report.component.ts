import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-tdc-installment-report',
  templateUrl: './tdc-installment-report.component.html',
  styles: []
})
export class TdcInstallmentReportComponent implements OnInit {
  @Input() selectedItems: NzSafeAny;
  data: any[] = [];
  dataExcelTables: any[] = [];
  newDate :any;

  constructor(private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository) {}

  ngOnInit(): void {
    this.getDataReport();
    console.log(this.selectedItems);
  }

  async getDataReport() {
    for (let i = 0; i < this.selectedItems.length; i++) {
      let newDate=new Date();
      let defaultDate=new Date(this.selectedItems[i].FirstPayDate);
      defaultDate.setFullYear(newDate.getFullYear());
      if(defaultDate<newDate)
      defaultDate.setFullYear(defaultDate.getFullYear()+1);
      this.newDate=convertDate(defaultDate.toString())

      const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(this.selectedItems[i].Id,  this.newDate, false);
      if (resp.meta?.error_code == 200) {
        this.dataExcelTables = resp.data.concat(this.dataExcelTables);
        const resp1 = await this.tDCInstallmentPriceRepository.GetReportTable(this.selectedItems[i].Id, this.dataExcelTables);
        if (resp1.meta?.error_code == 200) {
          this.data = resp1.data.concat(this.data);
        }
      }
    }
  }
  async exportReport() {
    console.log(this.data);
    const resp1 = await this.tDCInstallmentPriceRepository.ExportReport(this.data);
  }

  btn() {
    console.log(this.data);
  }
}
