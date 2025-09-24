import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { IngredientsPriceRepository } from 'src/app/infrastructure/repositories/ingredients-price.repository';

@Component({
  selector: 'app-tdc-project-ingreprice',
  templateUrl: './tdc-project-ingreprice.component.html',
  styles: []
})
export class TdcProjectIngrepriceComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() tDCProjectIngrePrices: any[] = [];

  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Output() eventDeletedEmitter: EventEmitter<any> = new EventEmitter();

  data_tableItemRef: any;
  invalid_tableItemRef = true;
  ingre_price_data: any[] = [];
  selectedValue: string = '';

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'IngredientsPriceId', className: 'text-center', width: 500 },
    { renderTitle: 'ValueHeader', render: 'Value', className: 'text-center', width: 150 },
    { renderTitle: 'LocationHeader', render: 'Location', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.updateTableItemRefRow(record, true)
        },
        {
          icon: 'delete',
          iif: i => !i.edit,
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá bản ghi này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.deleteItem(record)
        },
        {
          text: `Lưu`,
          iif: i => i.edit,
          type: 'link',
          click: record => {
            this.submit(record);
          }
        },
        {
          text: `Hủy`,
          iif: i => i.edit,
          click: record => this.cancelUpdateTableItemRefRow(record, false)
        }
      ]
    }
  ];
  constructor(
    private message: NzMessageService,
    private fb: FormBuilder,
    private ingredientsPriceRepository: IngredientsPriceRepository,
    private modalSrv: NzModalService
  ) {}

  ngOnInit(): void {
    this.data_tableItemRef = [...this.tDCProjectIngrePrices];
    this.invalid_tableItemRef = this.tDCProjectIngrePrices.length == 0 ? true : false;
    this.getIngreProjectData();
    this.orderByLocation();
  }
  orderByLocation() {
    this.tDCProjectIngrePrices = this.tDCProjectIngrePrices.sort((a, b) => a.Location - b.Location);
  }
  getValue() {
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any) => {
      res.push({
        IngredientsPriceId: item.IngredientsPriceId,
        Value: item.Value,
        Location: item.Location
      });
    });
    return res;
  }

  async getIngreProjectData() {
    let paging = new GetByPageModel();
    paging.order_by = 'CreatedAt Desc';
    const resp = await this.ingredientsPriceRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.ingre_price_data = resp.data;
      this.ingre_price_data.forEach((value: any) => {
        value.disabled = false;
      });
      if (this.ingre_price_data) localStorage.setItem('data', JSON.stringify(this.ingre_price_data));
    }
  }

  uploadIngrePriceData() {
    //this.getIngreProjectData();
    this.ingre_price_data.forEach((value: any) => {
      value.disabled = false;
    });
    let datatable: any[] = [];
    datatable = [...this.tableItemRef._data];
    this.ingre_price_data.forEach((value, index) => {
      datatable.forEach((val, i) => {
        if (value.Id == val.IngredientsPriceId) this.ingre_price_data[index].disabled = true;
      });
    });
    this.ingre_price_data = [...this.ingre_price_data];
  }

  onSelect(index: number, value: string) {
    this.selectedValue = value;
    const selectedOption = this.ingre_price_data.find(option => option.Id === value);
    if (selectedOption) {
      this.tableItemRef.setRow(index, { IngredientsPriceId: selectedOption.Id, IngrePriceName: selectedOption.Name });
    }
  }

  addRow() {
    let row = {
      IngredientsPriceId: undefined,
      Value: 0,
      Location: undefined,
      edit: true,
      index: this.data_tableItemRef.length + 1
    };
    this.uploadIngrePriceData();
    this.tableItemRef.addRow(row);
    this.data_tableItemRef.push(Object.assign({}, row));
    this.checkTableItemRefIsValid();
  }

  checkTableItemRefIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = true;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
      this.invalid_tableItemRef = isValid.length > 0 ? true : false;
    }
    this.tableItemRef._data = this.tableItemRef._data.sort((a, b) => a['Location'] - b['Location']);
    this.data_tableItemRef = this.data_tableItemRef.sort((a: any, b: any) => a['Location'] - b['Location']);

    this.eventEmitter.emit(this.tableItemRef._data);
  }

  tableItemRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        break;
      case 'dblClick':
        break;
    }
  }

  private updateTableItemRefRow(i: STData, edit: boolean): void {
    this.uploadIngrePriceData();
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }

  async deleteItem(i: STData) {
    this.eventDeletedEmitter.emit(i);
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
    this.uploadIngrePriceData();
  }

  private submit(i: STData): void {
    if (!i['IngredientsPriceId'] || !i['Value'] || !i['Location']) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
        this.uploadIngrePriceData();
        if (item.index == i['index']) {
          return Object.assign({}, i);
        } else return item;
      });
      this.updateTableItemRefRow(i, false);
      this.message.success(`Lưu item thành công!`);
    }
    this.checkTableItemRefIsValid();
  }

  private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!item['IngredientsPriceId'] || !item['Value'] || !item['Location']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.uploadIngrePriceData();
    this.checkTableItemRefIsValid();
  }
}
