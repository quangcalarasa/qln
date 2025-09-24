import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  selector: 'app-house-propose',
  templateUrl: './house-propose.component.html',
  styles: [
  ]
})
export class HouseProposeComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Input() data: any[] = [];
  data_tableItemRef: any = [];
  idxRow: number = -1;
  invalid_tableItemRef = true;
  selectedValue: string = '';

  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'DateHeader', render: 'Date', className: 'text-center', },
    { renderTitle: 'ProposeOptionHeader', render: 'ProposeOption', className: 'text-center', },
    { renderTitle: 'AuthLetterHeader', render: 'AuthLetter', className: 'text-center', width: 400 },
    { renderTitle: 'BrowseStatusHeader', render: 'BrowseStatus', className: 'text-center' },
    { renderTitle: 'BrowseDateHeader', render: 'BrowseDate', className: 'text-center' },
    { renderTitle: 'NoteHeader', render: 'Note', className: 'text-center' },
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

  constructor(private message: NzMessageService,
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private uploadRepository: UploadRepository,
    private userService: UserService
  ) { }

  ngOnInit(): void {
    if (this.data)
      for (let i = 0; i < this.data.length; i++) {
        this.data[i] = {
          Md167HouseId: this.data[i].Md167HouseId,
          Date: convertDate(this.data[i].Date),
          AuthLetter: this.data[i].AuthLetter,
          ProposeOption: this.data[i].ProposeOption,
          BrowseStatus: this.data[i].BrowseStatus,
          BrowseDate: convertDate(this.data[i].BrowseDate),
          Note: this.data[i].Note,
          index: i + 1,
          edit: false
        }
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
  addRow() {
    let row = {
      Md167HouseId: undefined,
      Date: undefined,
      ProposeOption: undefined,
      AuthLetter: undefined,
      BrowseStatus: undefined,
      BrowseDate: undefined,
      Note: undefined,
      index: this.data_tableItemRef.length + 1,
      edit: true
    };
    this.tableItemRef.addRow(row);
    this.data_tableItemRef.push(Object.assign({}, row));
    this.checkTableItemRefIsValid();
  }
  private updateTableItemRefRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }
  getValue() {
    return this.tableItemRef._data;
  }
  async deleteItem(i: STData) {
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
  }
  private submit(i: STData): void {
    if (!i['Date'] || !i['ProposeOption'] || !i['BrowseStatus'] || !i['BrowseDate'] || !i['ProposeOption']) {
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
    this.eventEmitter.emit();
  }
  checkTableItemRefIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = true;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
      this.invalid_tableItemRef = isValid.length > 0 ? true : false;
    }
  }
  private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!item['Date'] || !item['ProposeOption'] || !item['BrowseStatus'] || !item['BrowseDate']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.checkTableItemRefIsValid();
  }
  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadFile(formData);
    this.tableItemRef.setRow(this.idxRow, { AuthLetter: resp?.data.toString() })

    // this.validateForm.get('Attactment')?.setValue(resp?.data.toString());
  }

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }
}
