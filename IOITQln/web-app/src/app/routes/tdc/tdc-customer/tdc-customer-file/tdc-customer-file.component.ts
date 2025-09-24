import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { Inject, Injectable } from '@angular/core';
import { NzUploadChangeParam } from 'ng-zorro-antd/upload';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  selector: 'app-tdc-customer-file',
  templateUrl: './tdc-customer-file.component.html'
})
export class TdcCustomerFileComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() tdcCustomerFiles: any[] = [];

  data_tableItemRef: any;
  invalid_tableItemRef = true;
  token = this.tokenService.get()?.token;
  currItemUpload: any;

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'FileNameHeader', render: 'FileName' },
    { renderTitle: 'NoteHeader', render: 'Note' },
    { renderTitle: 'FileHeader', render: 'File' },
    {
      title: 'Chức năng',
      width: 150,
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
    private uploadRepository: UploadRepository,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.data_tableItemRef = [...this.tdcCustomerFiles];
    this.invalid_tableItemRef = this.tdcCustomerFiles.length == 0 ? true : false;
  }
  getValue() {
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any) => {
      res.push({
        FileName: item.FileName,
        Note: item.Note,
        File: item.File
      });
    });
    return res;
  }

  addRow() {
    let row = {
      FileName: undefined,
      Note: undefined,
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
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTableItemRefIsValid();
  }
  async deleteItem(i: STData) {
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
  }
  private submit(i: STData): void {
    if (!i['FileName'] || !i['File']) {
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
    this.checkTableItemRefIsValid();
  }
  private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!i['FileName'] || !i['File']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }

    this.checkTableItemRefIsValid();
  }
  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file, this.currItemUpload);
    return false;
  };

  async handleChange(file: any, i: STData) {
    const formData = new FormData();
    formData.append(file.name, file);
    const resp = await this.uploadRepository.uploadFile(formData);

    i['File'] = resp?.data.toString();
    this.tableItemRef.setRow(i, Object.assign({}), { refreshSchema: true });
  }

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }
}
