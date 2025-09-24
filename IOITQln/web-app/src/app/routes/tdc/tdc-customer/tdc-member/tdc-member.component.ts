import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';

import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-tdc-member',
  templateUrl: './tdc-member.component.html',
  styles: []
})
export class TdcMemberComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() tdcMenberCustomerDatas: any[] = [];
  @Input() pdwTT_data: any[] = [];
  @Input() pdwLH_data: any[] = [];
  @Input() lane_dataTT: any[] = [];
  @Input() lane_dataLH: any[] = [];

  lane_dataTt: any[] = [];
  lane_dataLh: any[] = [];

  data_tableItemRef: any;
  invalid_tableItemRef = true;
  loading: boolean = false;
  validateForm!: FormGroup;

  lane_data: any;

  columnsItem: STColumn[] = [
    { renderTitle: 'Họ Tên', render: 'FullNameTpl' },
    { renderTitle: 'CCCD', render: 'CCCDTpl' },
    { renderTitle: 'Ngày Sinh', render: 'DobTpl' },
    /////
    // {
    //   title: '* Địa chỉ tạm trú',
    //   children: [
    //     { title: 'Số Nhà', render: 'AddressTtTpl' },
    //     { title: 'Tỉnh (Tp) /Quận (huyện) /Phường (xã)', render: 'PdwTtTpl', width: 250 },
    //     { title: 'Tên Đường', render: 'LaneTtTpl', width: 170 }
    //   ]
    // },
    // {
    //   title: '* Địa chỉ lưu trú',
    //   children: [
    //     { title: 'Số Nhà', render: 'AddressLhTpl' },
    //     { title: 'Tỉnh (Tp) /Quận (huyện) /Phường (xã)', render: 'PdwLhTpl', width: 250 },
    //     { title: 'Tên Đường', render: 'LaneLhTpl', width: 170 }
    //   ]
    // },
    { renderTitle: 'SDT', render: 'PhoneTpl' },
    { renderTitle: 'Email', render: 'EmailTpl' },
    { renderTitle: 'Ghi Chú', render: 'NoteTpl' },
    {
      title: 'Chức năng',
      width: 80,
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
            title: 'Bạn có chắc chắn muốn xoá hệ số điều chỉnh này?',
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
    private laneRepository: LaneRepository,
  ) {}

  ngOnInit(): void {
    // this.tdcMenberCustomerDatas.map((item: any) => {
    //   (item.PdwTTC = [item.ProvinceTt, item.DistrictTt, item.WardTt]), (item.PdwLHC = [item.ProvinceLh, item.DistrictLh, item.WardLh]);
    //   return item;
    // });
    this.getAllLaneData();
    this.data_tableItemRef = [...this.tdcMenberCustomerDatas];
    this.invalid_tableItemRef = this.tdcMenberCustomerDatas.length == 0 ? true : false;
  }

  async getAllLaneData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `1=1`;
    paging.select = 'Id,Name,Ward';

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_data = resp.data;
    }
  }

  getValue() {
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any) => {
      res.push({
        FullName: item.FullName,
        CCCD: item.CCCD,
        Dob: convertDate(item.Dob),
        Phone: item.Phone,
        // AddressTt: item.AddressTt,
        // AddressLh: item.AddressLh,
        // LaneTt: item.LaneTt,
        // WardTt: item.PdwTTC[2],
        // DistrictTt: item.PdwTTC[1],
        // ProvinceTt: item.PdwTTC[0],
        // LaneLh: item.LaneLh,
        // WardLh: item.PdwLHC[2],
        // DistrictLh: item.PdwLHC[1],
        // ProvinceLh: item.PdwLHC[0],
        Email: item.Email,
        Note: item.Note
      });
    });
    return res;
  }

  addRow() {
    let row = {
      Id: undefined,
      TdcCustomerId: undefined,
      FullName: undefined,
      CCCD: undefined,
      Dob: undefined,
      AddressTt: undefined,
      AddressLh: undefined,
      Phone: undefined,
      Note: undefined,
      edit: true,
      index: this.data_tableItemRef.length + 1
    };

    this.tableItemRef.addRow(row);
    this.data_tableItemRef.push(Object.assign({}, row));
    this.checkTableItemRefIsValid();
  }

  tableItemRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        break;
      case 'dblClick':
        break;
    }
  }

  checkTableItemRefIsValid() {
    if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = true;
    else {
      let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
      this.invalid_tableItemRef = isValid.length > 0 ? true : false;
    }
  }

  private submit(i: STData): void {
    if (!i['FullName'] || !i['CCCD'] || !i['Dob'] || !i['Phone'] ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else if (!i['Phone'].match(/(84|0[3|5|7|9])+([0-9]{8})\b/g)) {
      this.message.error(`Số điện thoại thành viên không đúng định dạng!`);
    } else if (!i['CCCD'].match(/^\d{12}$/)) {
      this.message.error(`Căn cước công dân thành viên không đúng định dạng!`);
    } else {
      this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
        if (item.index == i['index']) {
          return Object.assign({}, i);
        } else return item;
      });
      this.updateTableItemRefRow(i, false);
      this.message.success(`Lưu thành viên thành công!`);
    }
    this.checkTableItemRefIsValid();
  }

  async deleteItem(i: STData) {
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa thành viên thành công!`);
    this.checkTableItemRefIsValid();
  }

  private updateTableItemRefRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }

  private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
    let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

    if (!i['FullName'] || !i['CCCD'] || !i['Dob'] || !i['Phone'] ) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.checkTableItemRefIsValid();
  }
}
