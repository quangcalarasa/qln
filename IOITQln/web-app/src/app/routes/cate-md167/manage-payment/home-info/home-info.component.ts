import { Component, Input, Output, OnInit, EventEmitter, ViewChild } from '@angular/core';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import { Md167ManPaymentRepository } from 'src/app/infrastructure/repositories/md167-manage-payment.repository';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { TypeHouse167 } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-home-info',
  templateUrl: './home-info.component.html',
  styles: [
  ]
})
export class HomeInfoComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Input() housePayments: any[] = [];
  @Input() record: NzSafeAny;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
  @Output() eventDeletedEmitter: EventEmitter<any> = new EventEmitter();
  data_tableItemRef: any;
  invalid_tableItemRef = true;
  md167_house_data: any[] = [];
  selectedValue: string = '';
  loading: boolean = false;
  selectedTaxNN: number =0;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'HouseHeader', render: 'HouseId', className: 'text-center', width: 150 },
    { renderTitle: 'HouseNameHeader', render: 'HouseName', className: 'text-center', width: 150 },
    { renderTitle: 'fullAddressHeader', render: 'fullAddress', className:'text-left', width: 250},
    // { renderTitle: 'ProviceNameHeader', render: 'ProviceName', className: 'text-center' },
    // { renderTitle: 'DistrictNameHeader', render: 'DistrictName', className: 'text-center' },
    // { renderTitle: 'WardNameHeader', render: 'WardName', className: 'text-center' },
    { renderTitle: 'LaneNameHeader', render: 'LaneName', className: 'text-center', width: 150 },
    { renderTitle: 'TaxNNHeader', render: 'TaxNN', className: 'text-left', width: 150 },
    { renderTitle: 'PaidHeader', render: 'Paid', className: 'text-left', width: 150 },
    { renderTitle: 'DebtHeader', render: 'Debt', className: 'text-left', width: 150 },
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
    private md167HouseRepository: Md167HouseRepository,
    private md167ManPaymentRepository: Md167ManPaymentRepository,
    private modalSrv: NzModalService
  ) { }

  ngOnInit(): void {
    this.data_tableItemRef = [...this.housePayments];
    this.invalid_tableItemRef = this.housePayments.length == 0 ? true : false;
    this.getmd167House();
  }

  getValue(){
    let res: any[] = [];
    [...this.tableItemRef._data].forEach((item: any)=> {
      res.push({
        HouseId: item.HouseId,
        fullAddress: item.fullAddress,
        HouseName: item.HouseName,
        // ProviceName: item.ProviceName,
        // DistrictName: item.DistrictName,
        // WardName: item.WardName,
        LaneName: item.LaneName,
        TaxNN: item.TaxNN,
        Paid: item.Paid,
        Debt: item.Debt,
      });
    });
    return res;
  }

  async getmd167House(){
    // let paging = new GetByPageModel();
    const resp = await this.md167ManPaymentRepository.getKiosAndHouse();
    if(resp.meta?.error_code == 200){
      this.md167_house_data = resp.data;
      this.md167_house_data.forEach((value:any)=>{
        value.disabled = false;
      });
      if(this.md167_house_data) localStorage.setItem('data', JSON.stringify(this.md167_house_data));
      
    }
  }


  updateHouseData(){
    this.md167_house_data.forEach((value: any)=>{
      value.disabled = false;
    });
    let datatable: any[] = [];
    datatable = [...this.tableItemRef._data];
    this.md167_house_data.forEach((value, index) => {
      datatable.forEach((val, i) => {
        if (value.HouseId == val.HouseId) this.md167_house_data[index].disabled = true;
      });
    });
    this.md167_house_data = [...this.md167_house_data];
  }

  onSelect(index: number, value: string) {
    this.selectedValue = value;
    const selectedOption = this.md167_house_data.find(option => option.HouseId === value);
    if (selectedOption) {
      this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, HouseCode: selectedOption.HouseCode });
      this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, fullAddress: selectedOption.fullAddress });
      this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, HouseName: selectedOption.HouseName });
      // this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, ProviceName: selectedOption.ProviceName });
      // this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, DistrictName: selectedOption.DistrictName });
      // this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, WardName: selectedOption.WardName });
      this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, LaneName: selectedOption.LaneName });
      this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, TaxNN: selectedOption.TaxNN });
    }
  }

  ChangeTaxNN(index: number, taxnn: number){
    this.selectedTaxNN = taxnn;
    const selectedOption = this.md167_house_data.find(option => option.HouseId === taxnn);
    this.tableItemRef.setRow(index, { HouseId: selectedOption.HouseId, TaxNN: selectedOption.TaxNN })
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(index, {Debt: data[index]['TaxNN'] - data[index]['Paid']});
  }

  ChangePrice(i: any, paid: number) {
    this.tableItemRef.setRow(i, { Paid: paid });
    let data = this.tableItemRef._data;
    this.tableItemRef.setRow(i, {Debt: data[i]['TaxNN'] - data[i]['Paid']});
  }

  addRow() {
    let row = {
      HouseId: undefined,
      HouseName:undefined,
      fullAddress:undefined,
      // ProviceName:undefined,
      // DistrictName:undefined,
      // WardName:undefined,
      LaneName: undefined,
      TaxNN: 0,
      Paid:0,
      Debt:0,
      edit: true,
      index: this.data_tableItemRef.length + 1
    };
    this.updateHouseData();
    this.tableItemRef.addRow(row);
    this.data_tableItemRef.push(Object.assign({}, row));
    this.checkTableItemRefIsValid();
  }

  checkTableItemRefIsValid(){
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
    this.updateHouseData();
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
    this.checkTableItemRefIsValid();
  }

  async deleteItem(i: STData) {
    this.eventDeletedEmitter.emit(i);
    this.tableItemRef.removeRow(i);
    this.message.create('success', `Xóa item thành công!`);
    this.checkTableItemRefIsValid();
    this.updateHouseData();
  }

  private submit(i: STData): void {
    if (!i['HouseId'] || 
        !i['HouseName'] ||
        !i['fullAddress'] ||
        // !i['ProviceName'] ||
        // !i['DistrictName'] ||
        // !i['WardName'] ||
        !i['LaneName'] ||
        !i['TaxNN'] ||
        !i['Debt']) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
        this.updateHouseData();
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

    if (!item['HouseId'] || 
        !item['HouseName'] ||
        !item['fullAddress'] ||
        // !item['ProviceName'] ||
        // !item['DistrictName'] ||
        // !item['WardName'] ||
        !item['LaneName'] ||
        !item['TaxNN']   ||
        !item['Debt']) {
      this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
      this.tableItemRef.removeRow(i);
    } else {
      item.edit = false;
      this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    }
    this.updateHouseData();
    this.checkTableItemRefIsValid();
  }
}