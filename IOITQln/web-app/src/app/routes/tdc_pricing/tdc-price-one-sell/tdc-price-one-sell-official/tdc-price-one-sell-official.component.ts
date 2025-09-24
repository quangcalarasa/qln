import { Component, Input, Output, OnInit, ChangeDetectorRef, ViewChild, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { Inject, Injectable } from '@angular/core';
import { UserService } from 'src/app/core/services/user.service';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-tdc-price-one-sell-official',
  templateUrl: './tdc-price-one-sell-official.component.html',
  styles: [
  ]
})
export class TdcPriceOneSellOfficialComponent implements OnInit {
  @ViewChild('tableItemRef') public tableItemRef!: STComponent;
  @Input() tdcPriceOneSellOfficials: any[] = [];
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  data_tableItemRef: any[] = [];
  invalid_tableItemRef = true;
  selectedValue: string = '';

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'IngredientsPriceId'},
    { render: 'IngrePriceName', className: 'text-center' },
    { renderTitle: 'AreaHeader', render: 'Area'},
    { renderTitle: 'PriceHeader', render: 'Price'},
    { renderTitle: 'TotalHeader', render: 'Total'},
  ];
  constructor() {}

  ngOnInit(): void {
    if (this.tdcPriceOneSellOfficials == undefined) {
      this.tdcPriceOneSellOfficials = [];
    }else
    {
      this.data_tableItemRef = [...this.tdcPriceOneSellOfficials];
      
    }
  }

  ChangeArea(i: number, area: number) {
    this.tableItemRef.setRow(i, { Area: area });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Total: data[i]['Area'] * data[i]['Price'] });
  }

  ChangePrice(i: any, price: number) {
    this.tableItemRef.setRow(i, { Price: price });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Total: data[i]['Area'] * data[i]['Price'] });
  }
  tableChange() {
    this.eventEmitter.emit(this.tableItemRef._data);
  }

}
