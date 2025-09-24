import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { RentalPeriodContract167Enum, RentalPurposeContract167Enum, TypePriceContract167Enum, Contract167TypeEnum } from 'src/app/shared/utils/enums';
import { TypePriceContract167, RentalPeriodContract167, RentalPurposeContract167, PaymentPeriodContract167, ContractStatus167 } from 'src/app/shared/utils/consts';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167ReceiptRepository } from 'src/app/infrastructure/repositories/md167-receipt.repository';
import { PricePerMonthComponent } from '../price-per-month/price-per-month.component';
import { ValuationComponent } from '../valuation/valuation.component';
import { AuctionDecisionComponent } from '../auction-decision/auction-decision.component';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-add-or-update-md167-payment',
  templateUrl: './payment.component.html'
})
export class AddOrUpdateMd167PaymentComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

  @Input() record: any;
  data: any;

  loading: boolean = false;
  curr_date: Date = new Date();
  invalidTbl = false;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'ReceiptCodeTitle', render: 'ReceiptCodeTpl' },
    { renderTitle: 'DateOfPaymentTitle', render: 'DateOfPaymentTpl', className: 'text-center' },
    { renderTitle: 'DateOfReceiptTitle', render: 'DateOfReceiptTpl', className: 'text-center' },
    { renderTitle: 'AmountTitle', render: 'AmountTpl', className: 'text-right' },
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

  constructor(
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private md167ReceiptRepository: Md167ReceiptRepository,
    private modal: NzModalRef,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.getData();
  }

  close(): void {
    this.modal.close();
  }

  addRow() {
    let row = {
      Id: undefined,
      Md167ContractId: undefined,
      Amount: undefined,
      DateOfPayment: convertDate((new Date).toDateString()),
      DateOfReceipt: convertDate((new Date).toDateString()),
      ReceiptCode: undefined,
      edit: true,
      index: this.data.length + 1
    };

    this.tableItemRef.addRow(row, { index: row.index });
    this.data = [Object.assign({}, row)].concat(this.data);
    this.checkTblIsValid();
  }

  checkTblIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalidTbl = false;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
      this.invalidTbl = isValid.length > 0 ? true : false;
    }
  }

  private submit(i: STData): void {
    if (
      i['ReceiptCode'] == undefined ||
      i['ReceiptCode'] == '' ||
      i['DateOfPayment'] == undefined ||
      i['DateOfPayment']?.toString() == '' ||
      i['DateOfReceipt'] == undefined ||
      i['DateOfReceipt']?.toString() == '' ||
      i['Amount'] == undefined ||
      i['Amount']?.toString() == ''
    ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.data = this.data.map((item: any) => {
        if (item.index == i['index']) {
          i['edit'] = false;
          return Object.assign({}, i);
        } else return item;
      });

      this.updateRow(i, false);
      this.message.success(`Lưu thông tin phiếu thu thành công!`);
    }

    this.checkTblIsValid();
  }

  async deleteRow(i: STData) {
    this.tableItemRef.removeRow(i);
    this.data.splice(i, 1);
    this.message.create('success', `Xóa thông tin phiếu thu thành công!`);

    this.checkTblIsValid();
  }

  private updateRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTblIsValid();
  }

  private cancelUpdateRow(i: STData, edit: boolean): void {
    let item = this.data.find((x: any) => x.index == i['index']);

    if (
      i['ReceiptCode'] == undefined ||
      i['ReceiptCode'] == '' ||
      i['DateOfPayment'] == undefined ||
      i['DateOfPayment']?.toString() == '' ||
      i['DateOfReceipt'] == undefined ||
      i['DateOfReceipt']?.toString() == '' ||
      i['Amount'] == undefined ||
      i['Amount']?.toString() == ''
    ) {
      this.data = this.data.filter((x: any) => x != item);
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
        break;
    }
  }

  getValue() {
    return this.tableItemRef._data.sort((a: any, b: any) => b - a);
  }

  async submitForm() {
    this.loading = true;
    const resp = await this.md167ReceiptRepository.updateList(this.record.Id, this.data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.message.create('success', `Lưu thông tin thanh toán thành công!`);
      this.modal.triggerOk();
    } else {
      this.loading = false;
    }
  }

  //get danh sách thanh toán của hợp đồng
  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Md167ContractId=${this.record.Id}`;
    paging.order_by = "UpdatedAt Asc";

    const resp = await this.md167ReceiptRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.data = resp.data.map((item: any, index: number) => {
        item.index = index + 1;
        item.DateOfPayment = convertDate(item.DateOfPayment);
        item.DateOfReceipt = convertDate(item.DateOfReceipt);
        return item;
      });
    }
  }
}
