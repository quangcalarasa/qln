import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-md167-contract-auction-decision',
  templateUrl: './auction-decision.component.html'
})
export class AuctionDecisionComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];

  invalidTbl = true;
  auctionDecisions: any;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'DecisionTitle', render: 'DecisionTpl' },
    { renderTitle: 'PriceTitle', render: 'PriceTpl', className: 'text-right' },
    { renderTitle: 'AuctionUnitTitle', render: 'AuctionUnitTpl', className: 'text-right' },
    { renderTitle: 'DateEffectTitle', render: 'DateEffectTpl', className: 'text-center', width: 150 },
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

  constructor(private message: NzMessageService) { }

  ngOnInit(): void {
    this.auctionDecisions = [...this.data];
    this.invalidTbl = this.data.length == 0 ? true : false;
  }

  addRow() {
    let row = {
      Id: undefined,
      Md167ContractId: undefined,
      Decision: undefined,
      Price: undefined,
      AuctionUnit: undefined,
      DateEffect: undefined,
      edit: true,
      index: this.auctionDecisions.length + 1
    };

    this.tableItemRef.addRow(row, { index: row.index });
    this.auctionDecisions = [Object.assign({}, row)].concat(this.auctionDecisions);
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
      i['Decision'] == undefined ||
      i['Decision']?.toString() == '' ||
      i['Price'] == undefined ||
      i['Price']?.toString() == '' ||
      i['AuctionUnit'] == undefined ||
      i['AuctionUnit']?.toString() == '' ||
      i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.auctionDecisions = this.auctionDecisions.map((item: any) => {
        if (item.index == i['index']) {
          i['edit'] = false;
          return Object.assign({}, i);
        } else return item;
      });

      this.updateRow(i, false);
      this.message.success(`Lưu thông tin quyết định đấu giá thành công!`);
    }

    this.checkTblIsValid();
  }

  async deleteRow(i: STData) {
    this.tableItemRef.removeRow(i);
    this.auctionDecisions.splice(i, 1);
    this.message.create('success', `Xóa thông tin quyết định đấu giá thành công!`);

    this.checkTblIsValid();
  }

  private updateRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTblIsValid();
  }

  private cancelUpdateRow(i: STData, edit: boolean): void {
    let item = this.auctionDecisions.find((x: any) => x.index == i['index']);

    if (
      i['Decision'] == undefined ||
      i['Decision']?.toString() == '' ||
      i['Price'] == undefined ||
      i['Price']?.toString() == '' ||
      i['AuctionUnit'] == undefined ||
      i['AuctionUnit']?.toString() == '' ||
      i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.auctionDecisions = this.auctionDecisions.filter((x: any) => x != item);
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
    return this.tableItemRef._data.sort((a: any, b: any) => b - a);
  }
}
