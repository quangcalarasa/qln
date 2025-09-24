import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';

@Component({
  selector: 'app-district-allocasion-apartment',
  templateUrl: './district-allocasion-apartment.component.html',
  styles: [
  ]
})
export class DistrictAllocasionApartmentComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() districtAllocasionApartment: any[] = [];
  @Input() record: NzSafeAny;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Output() eventDeletedEmitter: EventEmitter<any> = new EventEmitter();

  data_tableItemRef: any;
  invalid_tableItemRef = true;
  district_allocasion_apartment_data: any[] = [];
  selectedValue: string = '';
  loading: boolean = false;
  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'DistrictHeader', render: 'DistrictId', className: 'text-center' },
    { renderTitle: 'ActualNumberHeader', render: 'ActualNumber', className: 'text-center' },
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
    private districtRepository: DistrictRepository,
    private modalSrv: NzModalService
  ) { }

  ngOnInit(): void {
    this.data_tableItemRef = [...this.districtAllocasionApartment];
    this.invalid_tableItemRef = this.districtAllocasionApartment.length == 0 ? true : false;
    this.getDistrictAllocasionData();
  }

  getValue() {
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any) => {
      res.push({
        DistrictId: item.DistrictId,
        ActualNumber: item.ActualNumber
      });
    });
    return res;
  }

  async getDistrictAllocasionData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.order_by = 'CreatedAt Desc';
    const resp = await this.districtRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.district_allocasion_apartment_data = resp.data;
      this.district_allocasion_apartment_data.forEach((value: any) => {
        value.disabled = false;
      });
      if (this.district_allocasion_apartment_data) localStorage.setItem('data', JSON.stringify(this.district_allocasion_apartment_data));
    }
  }

  uploadDistrictAllocasionData() {
    this.district_allocasion_apartment_data.forEach((value: any) => {
      value.disabled = false;
    });
    let datatable: any[] = [];
    datatable = [...this.tableItemRef._data];
    this.district_allocasion_apartment_data.forEach((value, index) => {
      datatable.forEach((val, i) => {
        if (value.Id == val.DistrictId) this.district_allocasion_apartment_data[index].disabled = false;
      });
    });
    this.district_allocasion_apartment_data = [...this.district_allocasion_apartment_data];
  }

  onSelect(index: number, value: string) {
    this.selectedValue = value;
    const selectedOption = this.district_allocasion_apartment_data.find(option => option.Id === value);
    if (selectedOption) {
      this.tableItemRef.setRow(index, { DistrictId: selectedOption.Id, DistrictName: selectedOption.Name });
    }
  }

  addRow() {
    let row = {
      DistrictId: undefined,
      ActualNumber: 0,
      edit: true,
      index: this.data_tableItemRef.length + 1
    };
    this.uploadDistrictAllocasionData();
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
    this.uploadDistrictAllocasionData();
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }

  async deleteItem(i: STData) {
    this.eventDeletedEmitter.emit(i);
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
    this.uploadDistrictAllocasionData();
  }

  private submit(i: STData): void {
    if (!i['DistrictId'] || !i['ActualNumber']) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
        this.uploadDistrictAllocasionData();
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

    if (!item['DistrictId'] || !item['ActualNumber']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.uploadDistrictAllocasionData();
    this.checkTableItemRefIsValid();
  }

}
