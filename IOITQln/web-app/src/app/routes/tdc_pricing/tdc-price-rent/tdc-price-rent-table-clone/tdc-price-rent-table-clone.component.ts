import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';

@Component({
  selector: 'app-tdc-price-rent-table-clone',
  templateUrl: './tdc-price-rent-table-clone.component.html',
  styles: []
})
export class TdcPriceRentTableCloneComponent implements OnInit {
  @Input() record: NzSafeAny;
  data: any[] = [];

  constructor(private tdcPriceRentRepository: TdcPriceRentRepository) { }

  ngOnInit(): void {
    this.getWorkSheet();
  }

  async getWorkSheet() {
    const resp = await this.tdcPriceRentRepository.getWorkSheet(this.record.Id);

    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }
}
