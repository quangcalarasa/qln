import { Component, Input, OnInit, ChangeDetectorRef, ViewChild, SimpleChanges } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { OriginalPriceAndTaxRepository } from 'src/app/infrastructure/repositories/original-price-and-tax.repository';
import { IngredientsPriceRepository } from 'src/app/infrastructure/repositories/ingredients-price.repository';
@Component({
  selector: 'app-tdc-project-price-and-tax',
  templateUrl: './tdc-project-price-and-tax.component.html',
  styles: []
})
export class TdcProjectPriceAndTaxComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() tdcProjectPriceAndTax: any[] = [];
  @Input() tDCProjectIngrePrices: any[] = [];
  @Input() DeletedItemIngrePrice: any;

  add: boolean = true;
  data_tableItemRef: any;
  invalid_tableItemRef = true;
  ingre_price_data: any[] = [];
  originPAT: any[] = [];
  IngrePrice: any[] = [];
  selectedValue: string = '';
  listOfData: any[] = [];

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'IngrePriceHeader', render: 'tDCProjectPriceAndTaxDetails', className: 'text-center', width: 250 },
    { renderTitle: 'PATHeader', render: 'PriceAndTaxId', className: 'text-center', width: 250 },
    { renderTitle: 'ValueHeader', render: 'Value', className: 'text-center', width: 150 },
    { renderTitle: 'LocationHeader', render: 'Location', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit1,
          click: record => this.updateTableItemRefRow(record, true)
        },
        {
          icon: 'delete',
          iif: i => !i.edit1,
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
          iif: i => i.edit1,
          type: 'link',
          click: record => {
            this.submit(record);
          }
        },
        {
          text: `Hủy`,
          iif: i => i.edit1,
          click: record => this.cancelUpdateTableItemRefRow(record, false)
        }
      ]
    }
  ];
  constructor(
    private message: NzMessageService,
    private fb: FormBuilder,
    private ingredientsPriceRepository: IngredientsPriceRepository,
    private originalPriceAndTaxRepository: OriginalPriceAndTaxRepository,
    private modalSrv: NzModalService
  ) {}

  ngOnInit(): void {
    this.data_tableItemRef = [...this.tdcProjectPriceAndTax];
    this.ingre_price_data = [...this.tDCProjectIngrePrices];
    this.invalid_tableItemRef = this.tdcProjectPriceAndTax.length == 0 ? true : false;
    this.getPATProjectData();
    this.getIPProjectData();
    if (localStorage.getItem('add') == 'false') this.add = false;
    this.orderByLocation();
  }

  getValue() {
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any) => {
      res.push({
        tDCProjectPriceAndTaxDetails: item.TDCProjectPriceAndTaxDetails,
        PriceAndTaxId: item.PriceAndTaxId,
        Value: item.Value,
        Location: item.Location
      });
    });
    return res;
  }

  changeData(items: any) {
    if (items.length == 0) {
      this.listOfData = [];
    } else {
      this.listOfData = [...this.tDCProjectIngrePrices];
    }
  }

  receiveDeletedData(data: any) {
    let newData = this.tdcProjectPriceAndTax;

    for (let i = 0; i < newData.length; i++) {
      let item = newData[i];
      item.TDCProjectPriceAndTaxDetails = item.TDCProjectPriceAndTaxDetails.filter(
        (s: any) => s.IngredientsPriceId != data.IngredientsPriceId
      );
      newData[i] = item;
    }

    this.tdcProjectPriceAndTax = [...newData.filter((s: any) => s.TDCProjectPriceAndTaxDetails.length)];
  }

  ngOnchange(changes: SimpleChanges) {
    this.changeData(this.tDCProjectIngrePrices);
  }

  async getPATProjectData() {
    let paging = new GetByPageModel();
    paging.order_by = 'CreatedAt Desc';
    const resp = await this.originalPriceAndTaxRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.originPAT = resp.data;
      this.originPAT.forEach((value: any) => {
        value.disabled = false;
      });
    }
  }
  async getIPProjectData() {
    let paging = new GetByPageModel();
    paging.order_by = 'CreatedAt Desc';
    const resp = await this.ingredientsPriceRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.IngrePrice = resp.data;
    }
  }

  uploadPATData() {
    //this.getIngreProjectData();
    this.originPAT.forEach((value: any) => {
      value.disabled = false;
    });
    let datatable: any[] = [];
    datatable = [...this.tableItemRef._data];
    this.originPAT.forEach((value, index) => {
      datatable.forEach((val, i) => {
        if (value.Id == val.PriceAndTaxId) this.originPAT[index].disabled = true;
      });
    });
    this.originPAT = [...this.originPAT];
  }

  onSelect(index: number, value: string) {
    this.selectedValue = value;
    const selectedOption = this.originPAT.find(option => option.Id === value);
    if (selectedOption) {
      this.tableItemRef.setRow(index, { PriceAndTaxId: selectedOption.Id, PATName: selectedOption.Name });
    }
  }
  onSelectIngre(item: STData, value: any) {
    // this.tableItemRef.setRow(index, { TDCProjectPriceAndTaxDetails: [] })
    item['TDCProjectPriceAndTaxDetails'] = value;
    this.tableItemRef.setRow(item, { item });
  }
  addRow() {
    this.ingre_price_data = [...this.tDCProjectIngrePrices];
    let row = {
      tDCProjectPriceAndTaxDetails: [],
      PriceAndTaxId: undefined,
      Value: 0,
      Location: undefined,
      edit1: true,
      index: this.data_tableItemRef.length + 1
    };

    this.tableItemRef.addRow(row);
    this.data_tableItemRef.push(Object.assign({}, row));
    this.checkTableItemRefIsValid();
    this.uploadPATData();
  }

  checkTableItemRefIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = true;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit1'] == true);
      this.invalid_tableItemRef = isValid.length > 0 ? true : false;
    }
    this.tableItemRef._data = this.tableItemRef._data.sort((a, b) => a['Location'] - b['Location']);
    this.data_tableItemRef = this.data_tableItemRef.sort((a: any, b: any) => a['Location'] - b['Location']);
  }

  tableItemRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        break;
      case 'dblClick':
        break;
    }
  }

  orderByLocation() {
    this.tdcProjectPriceAndTax = this.tdcProjectPriceAndTax.sort((a, b) => a.Location - b.Location);
  }

  private updateTableItemRefRow(i: STData, edit1: boolean): void {
    this.ingre_price_data = [...this.tDCProjectIngrePrices];
    this.tableItemRef.setRow(i, { edit1 }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
    this.uploadPATData();
  }

  async deleteItem(i: STData) {
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
    this.uploadPATData();
  }

  private submit(i: STData): void {
    if (!i['PriceAndTaxId'] || !i['Value'] || !i['Location'] || !i['TDCProjectPriceAndTaxDetails']) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
        if (item.index == i['index']) {
          return Object.assign({}, i);
        } else return item;
      });
      this.updateTableItemRefRow(i, false);

      this.message.success(`Lưu item thành công!`);
    }
    this.uploadPATData();
    this.checkTableItemRefIsValid();
  }

  private cancelUpdateTableItemRefRow(i: STData, edit1: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!item['PriceAndTaxId'] || !item['Value'] || !item['Location']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit1 = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.uploadPATData();
    this.checkTableItemRefIsValid();
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.IngredientsPriceId == o2.IngredientsPriceId : o1 === o2;
  };
}
