import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';

@Component({
  selector: 'app-tdc-installment-temporary',
  templateUrl: './tdc-installment-temporary.component.html',
  styles: []
})
export class TdcInstallmentTemporaryComponent implements OnInit {
  @ViewChild('tableItemRef') public tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Input() IngrePrices: any[] = [];

  data_tableItemRef: any[] = [];

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'IngredientsPriceId', className: 'text-center' },
    { render: 'IngrePriceName', className: 'text-center' },
    { renderTitle: 'AreaHeader', render: 'Area', className: 'text-center' },
    { renderTitle: 'UnitPriceHeader', render: 'UnitPrice', className: 'text-center' },
    { renderTitle: 'PriceHeader', render: 'Price', className: 'text-center' }
  ];
  constructor() { }

  ngOnInit(): void {
    this.data_tableItemRef = [...this.IngrePrices];
  }

  changeArea(i: number, area: any) {
    this.tableItemRef.setRow(i, { Area: area });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Price: data[i]['Area'] * data[i]['UnitPrice'] });
    this.tableItemRefChange()

  }

  changUnitPrice(i: any, unitPrice: any) {
    this.tableItemRef.setRow(i, { UnitPrice: unitPrice });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, { Price: data[i]['Area'] * data[i]['UnitPrice'] });
    this.tableItemRefChange()
  }

  tableItemRefChange() {
    this.eventEmitter.emit(this.tableItemRef._data);
  }

  getTotalPrice() {
    let data = this.tableItemRef._data;
    let count = 0;
    for (let i = 0; i < data.length; i++) {
      count += data[i]['Price'];
    }
    return count;
  }
}
