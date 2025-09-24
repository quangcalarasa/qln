import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-pricing-reduced-person',
  templateUrl: './reduced-person.component.html'
})

export class ReducedPersonComponent implements OnInit {
  @ViewChild('tableItemRef') public tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];
  @Input() deduction_coefficient_data: any[] = [];
  @Input() customer_data: any[] = [];
  @Input() salary_default?: number = undefined;

  data_tableItemRef: any;
  invalid_tableItemRef = true;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'nameHeader', render: 'nameTpl', className: "text-center" },
    { renderTitle: 'yearHeader', render: 'yearTpl', className: "text-center" },
    { renderTitle: 'salaryHeader', render: 'salaryTpl', className: "text-center" },
    { renderTitle: 'coefficientHeader', render: 'coefficientTpl', className: "text-center" },
    { renderTitle: 'valueHeader', render: 'valueTpl', className: "text-center" },
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

  constructor(private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.data_tableItemRef = [...this.data];
    this.invalid_tableItemRef = this.data.length == 0 ? true : false;
  }

  addRow() {
    let row = {
      Id: undefined,
      CustomerId: undefined,
      Year: undefined,
      Salary: this.salary_default,
      Value: undefined,
      DeductionCoefficient: undefined,
      edit: true,
      index: this.data_tableItemRef.length + 1
    };

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

    this.eventEmitter.emit(this.invalid_tableItemRef);
  }

  private submit(i: STData): void {
    if (!i['CustomerId'] || !i['Year'] || !i['Salary'] || !i['DeductionCoefficient']) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    }
    else {
      //check khách hàng đã tồn tại
      let customerId = i['CustomerId'];
      let exist = this.data_tableItemRef.find((x: any) => x.CustomerId == customerId && x.index != i['index']);
      if (exist) {
        this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
        this.message.error(`Người được tinh giảm đã tồn tại!`);
      }
      else {
        i['Value'] = Math.round(i['Year'] * i['Salary'] * i['DeductionCoefficient']);

        this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
          if (item.index == i['index']) {
            return Object.assign({}, i);
          } else return item;
        });

        this.updateTableItemRefRow(i, false);
        this.message.success(`Lưu người được tinh giảm thành công!`);
      }
    }

    this.checkTableItemRefIsValid();
  }

  async deleteItem(i: STData) {
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa người được tinh giảm thành công!`);

    this.checkTableItemRefIsValid();
  }

  private updateTableItemRefRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTableItemRefIsValid();
  }

  private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!i['CustomerId'] || !i['Year'] || !i['Salary'] || !i['DeductionCoefficient'] || !i['Value']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }

    this.checkTableItemRefIsValid();
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
}
