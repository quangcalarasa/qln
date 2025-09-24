import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  selector: 'app-md167-contract-valuation',
  templateUrl: './valuation.component.html'
})
export class ValuationComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];

  invalidTbl = true;
  valuations: any;

  idxRow: number = -1;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'unitValuationTitle', render: 'unitValuationTpl' },
    { renderTitle: 'attactmentTitle', render: 'attactmentTpl' },
    { renderTitle: 'dateEffectTitle', render: 'dateEffectTpl', className: 'text-center', width: 150 },
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

  constructor(private message: NzMessageService, private uploadRepository: UploadRepository, private userService: UserService) { }

  ngOnInit(): void {
    this.valuations = [...this.data];
    this.invalidTbl = this.data.length == 0 ? true : false;
  }

  addRow() {
    let row = {
      Id: undefined,
      Md167ContractId: undefined,
      UnitValuation: undefined,
      Attactment: undefined,
      DateEffect: undefined,
      edit: true,
      index: this.valuations.length + 1
    };

    this.tableItemRef.addRow(row, { index: row.index });
    this.valuations = [Object.assign({}, row)].concat(this.valuations);
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
      i['UnitValuation'] == undefined ||
      i['UnitValuation'] == '' ||
      i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.valuations = this.valuations.map((item: any) => {
        if (item.index == i['index']) {
          i['edit'] = false;
          return Object.assign({}, i);
        } else return item;
      });

      this.updateRow(i, false);
      this.message.success(`Lưu thông tin thẩm định giá thành công!`);
    }

    this.checkTblIsValid();
  }

  async deleteRow(i: STData) {
    this.tableItemRef.removeRow(i);
    this.valuations.splice(i, 1);
    this.message.create('success', `Xóa thông tin thẩm định giá thành công!`);

    this.checkTblIsValid();
  }

  private updateRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTblIsValid();
  }

  private cancelUpdateRow(i: STData, edit: boolean): void {
    let item = this.valuations.find((x: any) => x.index == i['index']);

    if (
      i['UnitValuation'] == undefined ||
      i['UnitValuation'] == '' ||
      i['DateEffect'] == undefined ||
      i['DateEffect']?.toString() == ''
    ) {
      this.valuations = this.valuations.filter((x: any) => x != item);
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

  beforeUpload = (file: NzUploadFile): boolean => {
    console.log(file);
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadFile(formData);
    this.tableItemRef.setRow(this.idxRow, { Attactment: resp?.data.toString() })
    // this.validateForm.get('Attactment')?.setValue(resp?.data.toString());
  }

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }
}
