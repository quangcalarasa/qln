import { Component, Input, Output, OnInit, ChangeDetectorRef, ViewChild, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
@Component({
  selector: 'app-tdc-price-rent-temporary',
  templateUrl: './tdc-price-rent-temporary.component.html'
})
export class TdcPriceRentTemporaryComponent implements OnInit {
  @ViewChild('tableItemRef') public tableItemRef!: STComponent;
  @Input() tdcPriceRentTemporaries: any[] = [];
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  data_tableItemRef: any[] = [];
  invalid_tableItemRef = true;
  selectedValue: string = '';

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'IngredientsPriceId' },
    { render: 'IngrePriceName', className: 'text-center' },
    { renderTitle: 'AreaHeader', render: 'Area' },
    { renderTitle: 'PriceHeader', render: 'Price' },
    { renderTitle: 'TotalHeader', render: 'Total' }
  ];
  constructor() {}

  ngOnInit(): void {
    if (this.tdcPriceRentTemporaries == undefined) {
      this.tdcPriceRentTemporaries = [];
    } else {
      this.data_tableItemRef = [...this.tdcPriceRentTemporaries];
    }
  }

  ChangeArea(i: number, area: number) {
    this.tableItemRef.setRow(i, { Area: area });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Total: data[i]['Area'] * data[i]['Price'] });
    this.tableChange();
  }

  ChangePrice(i: any, price: number) {
    this.tableItemRef.setRow(i, { Price: price });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Total: data[i]['Area'] * data[i]['Price'] });
    this.tableChange();
  }
  tableChange() {
    this.eventEmitter.emit(this.tableItemRef._data);
  }
}
