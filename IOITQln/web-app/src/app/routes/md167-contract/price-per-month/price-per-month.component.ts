import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { Md167VATValueRepository } from 'src/app/infrastructure/repositories/md167vat-value.repository';

@Component({
  selector: 'app-md167-contract-price-per-month',
  templateUrl: './price-per-month.component.html'
})
export class PricePerMonthComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];

  invalidTbl = true;
  pricePerMonths: any;

  vat_data: any[] = [];

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'totalPriceTitle', render: 'totalPriceTpl', className: 'text-right' },
    { renderTitle: 'housePriceTitle', render: 'housePriceTpl', className: 'text-right' },
    { renderTitle: 'landPriceTitle', render: 'landPriceTpl', className: 'text-right' },
    { renderTitle: 'vatPriceTitle', render: 'vatPriceTpl', className: 'text-right' },
    { renderTitle: 'dateEffectTitle', render: 'dateEffectTpl', className: 'text-center', width: 200 },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.updateRow(record, true)
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
          click: record => this.deleteRow(record)
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
          click: record => this.cancelUpdateRow(record, false)
        }
      ]
    }
  ];

  constructor(private message: NzMessageService, private md167VATValueRepository: Md167VATValueRepository) { }

  ngOnInit(): void {
    this.getVatData();
    this.pricePerMonths = [...this.data];
    this.invalidTbl = this.data.length == 0 ? true : false;
  }

  addRow() {
    let row = {
      Id: undefined,
      Md167ContractId: undefined,
      TotalPrice: undefined,
      HousePrice: undefined,
      LandPrice: undefined,
      VatPrice: undefined,
      DateEffect: undefined,
      VatValue: undefined,
      edit: true,
      index: this.pricePerMonths.length + 1
    };

    this.tableItemRef.addRow(row, { index: row.index });
    this.pricePerMonths = [Object.assign({}, row)].concat(this.pricePerMonths);
    this.checkTblIsValid();
  }

  checkTblIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalidTbl = false;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
      this.invalidTbl = isValid.length > 0 ? true : false;
    }

    this.eventEmitter.emit(this.invalidTbl);
  }

  private submit(i: STData): void {
    if (
      i['TotalPrice'] == undefined ||
      i['TotalPrice']?.toString() == '' ||
      i['LandPrice'] == undefined ||
      i['LandPrice']?.toString() == '' || i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      //Tính tiền thuê nhà và vat
      let vat = 0;

      let date = (new Date(i['DateEffect'])).getTime();
      for (let idx = 0; idx < this.vat_data.length; idx++) {
        let vat_data_item = this.vat_data[idx];
        let dateItem = (new Date(vat_data_item.EffectiveDate)).getTime();
        if (dateItem < date) {
          vat = vat_data_item.Value / 100;
          i['VatValue'] = vat_data_item.Value;
          break;
        }
      }

      let housePriceIncludeVat = i['TotalPrice'] - i['LandPrice'];

      i['HousePrice'] = Math.round(housePriceIncludeVat / (1 + vat));
      i['VatPrice'] = housePriceIncludeVat - i['HousePrice'];

      this.pricePerMonths = this.pricePerMonths.map((item: any) => {
        if (item.index == i['index']) {
          i['edit'] = false;
          return Object.assign({}, i);
        } else return item;
      });

      this.updateRow(i, false);
      this.message.success(`Lưu thông tin số tiền/tháng thành công!`);
    }

    this.checkTblIsValid();
  }

  async deleteRow(i: STData) {
    this.tableItemRef.removeRow(i);
    this.pricePerMonths.splice(i, 1);
    this.message.create('success', `Xóa thông tin số tiền/tháng thành công!`);

    this.checkTblIsValid();
  }

  private updateRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTblIsValid();
  }

  private cancelUpdateRow(i: STData, edit: boolean): void {
    let item = this.pricePerMonths.find((x: any) => x.index == i['index']);

    if (
      i['TotalPrice'] == undefined ||
      i['TotalPrice']?.toString() == '' ||
      i['LandPrice'] == undefined ||
      i['LandPrice']?.toString() == '' || i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.pricePerMonths = this.pricePerMonths.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }

    this.checkTblIsValid();
  }

  tableItemRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        break;
      case 'dblClick':
        // this.openAddTypeAttributeItem(undefined);
        break;
    }
  }

  getValue() {
    return this.tableItemRef._data;
  }

  async getVatData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.order_by = "EffectiveDate Desc";

    const resp = await this.md167VATValueRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.vat_data = resp.data;
    }
  }
}
