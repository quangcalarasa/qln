import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { AddorupdateMd167KiosComponent } from './addorupdate/addorupdate.component';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { Md167KiosRepository } from 'src/app/infrastructure/repositories/md167kios.repository';

@Component({
  selector: 'app-kios',
  templateUrl: './kios.component.html',
  styles: [
  ]
})
export class KiosComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Input() data: any[] = [];
  @Input() dataHouse: any;

  data_tableItemRef: any = [];
  invalid_tableItemRef = true;
  selectedValue: string = '';
  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'HouseNumberHeader', render: 'HouseNumber', className: 'text-center', },
    { renderTitle: 'UseFloorPbHeader', render: 'UseFloorPb', className: 'text-center', },
    { renderTitle: 'UseFloorPrHeader', render: 'UseFloorPr', className: 'text-center', },
    { renderTitle: 'KiosStatusNameHeader', render: 'KiosStatusName', className: 'text-center' },
    { renderTitle: 'NoteHeader', render: 'Note', className: 'text-center' },

    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          click: record => this.addOrUpdate(record)
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá bản ghi này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.deleteItem(record)
        }
      ]
    }
  ];

  constructor(private message: NzMessageService,
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private md167KiosRepository: Md167KiosRepository,
    private drawerRef: NzDrawerRef<string>,) { }

  ngOnInit(): void {
    if (this.data)
      for (let i = 0; i < this.data.length; i++) {
        this.data[i] = {
          Id: this.data[i].Id,
          HouseNumber: this.data[i].HouseNumber,
          UseFloorPb: this.data[i].UseFloorPb,
          UseFloorPr: this.data[i].UseFloorPr,
          KiosStatus: this.data[i].KiosStatus,
          IsPayTax: this.data[i].IsPayTax??false,
          TaxNN: this.data[i].TaxNN,
          Code: this.data[i].Code,
          KiosStatusName: this.data[i].KiosStatus == 1 ? "Đã cho thuê" : "Chưa cho thuê",
          Note: this.data[i].Note,
          index: i + 1,
        }
      }
    else {
      this.data = [];
    }
  }
  private updateTableItemRefRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }
  addOrUpdate(record?: any): void {
    this.modalSrv.create({
      nzTitle: record ? `Sửa Kios "${this.data[record.index - 1].HouseNumber}"` : "Thêm mới Kios",
      nzContent: AddorupdateMd167KiosComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record: record,
        houseSelect: this.dataHouse
      },
      nzOnOk: (res: any) => {
        this.loadTable(res.validateForm.getRawValue());

        // this.drawerRef.close(true);
        this.message.create('success', `Thêm thành công!`);
      }
    });
  }
  loadTable(value: any) {
    console.log(value);
    
    if (value.index == null) {
      let newData = {
        Id: value.Id,
        HouseNumber: value.HouseNumber,
        IsPayTax:value.IsPayTax,
        UseFloorPb: value.UseFloorPb,
        UseFloorPr: value.UseFloorPr,
        KiosStatus: value.KiosStatus,
        TaxNN: value.TaxNN,
        Code: value.Code,
        KiosStatusName: value.KiosStatus == 1 ? "Đã cho thuê" : "Chưa cho thuê",
        Note: value.Note,
        index: this.data.length + 1,
      }
      this.data.push(newData);

    }
    else {
      this.data[value.index - 1].HouseNumber = value.HouseNumber;
      this.data[value.index - 1].UseFloorPb = value.UseFloorPb;
      this.data[value.index - 1].UseFloorPr = value.UseFloorPr;
      this.data[value.index - 1].KiosStatus = value.KiosStatus;
      this.data[value.index - 1].IsPayTax = value.IsPayTax;
      this.data[value.index - 1].TaxNN = value.TaxNN;
      this.data[value.index - 1].Note = value.Note;
      this.data[value.index - 1].KiosStatusName = value.KiosStatus == 1 ? "Đã cho thuê" : "Chưa cho thuê";

    }
    this.data = [...this.data]
    this.eventEmitter.emit()
  }
  getValue() {
    return this.data;
  }
  async deleteItem(i: STData) {
    if (i['Id'] > 0) {
      const resp = await this.md167KiosRepository.DeleteKios(i['Id']);
      if (resp.data == true) {
        this.tableItemRef.removeRow(i);
        this.message.create('success', `Xóa item thành công!`);
        this.checkTableItemRefIsValid();
      } else {
        this.modalSrv.error({
          nzTitle: resp.meta?.error_message
        });
      }
    }
    return
    // 

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

    if (!item['Date'] || !item['Code'] || !item['Content']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.checkTableItemRefIsValid();
  }
}
