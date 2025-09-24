import { Component, Input, OnChanges, OnInit, SimpleChanges, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { LevelBlock } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
@Component({
  selector: 'app-rent-block-detail',
  templateUrl: './rent-block-detail.component.html'
})
export class RentBlockDetailComponent implements OnInit, OnChanges {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

  @Input() data: any[] = [];
  @Input() levelblocks?: any[];

  area_data: any[] = [];
  floor_data: any[] = [];

  blockDetails: any;
  invalidTbl = true;

  level_data = LevelBlock;

  columnsItem: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { renderTitle: 'levelTitle', render: 'levelTpl' },
    { renderTitle: 'floorTitle', render: 'floorTpl' },
    { renderTitle: 'areaTitle', render: 'areaTpl' },
    { renderTitle: 'totalAreaDetailFloorTitle', render: 'totalAreaDetailFloorTpl', className: 'text-center' },
    { renderTitle: 'privateAreaTitle', render: 'privateAreaTpl', className: 'text-center' },
    { renderTitle: 'generalAreaTitle', render: 'generalAreaTpl', className: 'text-center' },
    { renderTitle: 'yardAreaTitle', render: 'yardAreaTpl', className: 'text-center' },
    { renderTitle: 'timeTitle', render: 'timeTpl', className: 'text-center', width: 150 },
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
            title: 'Bạn có chắc chắn muốn xoá chi tiết căn nhà này?',
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

  constructor(private message: NzMessageService, private floorRepository: FloorRepository, private areaRepository: AreaRepository) {}

  ngOnInit(): void {
    this.blockDetails = [...this.data];
    this.invalidTbl = this.data.length == 0 ? true : false;

    this.getDataArea();
    this.getDataFloor();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!changes['data']) {
      let data = this.tableItemRef._data;
      data.forEach(item => {
        if (item['Level']) {
          let rowMapLevelBlock = this.levelblocks?.find(x => x.key == item['Level'].toString() || x.LevelId == item['Level']);
          if (!rowMapLevelBlock) {
            this.tableItemRef.removeRow(item);
            this.blockDetails.splice(item, 1);
          }
        }
      });

      this.checkTblIsValid();
    }
  }

  addRow() {
    let row = {
      Id: undefined,
      Level: undefined,
      AreaId: undefined,
      FloorId: undefined,
      GeneralArea: undefined,
      PrivateArea: undefined,
      TotalAreaDetailFloor: undefined,
      YardArea: undefined,
      DisposeTime: undefined,
      edit: true,
      index: this.blockDetails.length + 1
    };

    this.tableItemRef.addRow(row, { index: row.index });
    this.blockDetails = [Object.assign({}, row)].concat(this.blockDetails);
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
      !i['Level'] ||
      !i['FloorId'] ||
      !i['AreaId'] ||
      i['GeneralArea'] == undefined ||
      i['GeneralArea']?.toString() == '' ||
      i['PrivateArea'] == undefined ||
      i['PrivateArea']?.toString() == '' ||
      i['YardArea'] == undefined ||
      i['YardArea']?.toString() == '' ||
      i['DisposeTime'] == undefined ||
      i['DisposeTime'] == ''
    ) {
      this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    } else {
      //Tính DT sử dụng
      i['TotalAreaDetailFloor'] = (i['GeneralArea'] ?? 0) + (i['PrivateArea'] ?? 0) + (i['YardArea'] ?? 0);

      this.blockDetails = this.blockDetails.map((item: any) => {
        if (item.index == i['index']) {
          return Object.assign({}, i);
        } else return item;
      });

      this.updateRow(i, false);
      this.message.success(`Lưu thông tin chi tiết căn nhà thành công!`);
    }

    this.checkTblIsValid();
  }

  async deleteRow(i: STData) {
    this.tableItemRef.removeRow(i);
    this.blockDetails.splice(i, 1);
    this.message.create('success', `Xóa chi tiết căn nhà thành công!`);

    this.checkTblIsValid();
  }

  private updateRow(i: STData, edit: boolean): void {
    this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    this.checkTblIsValid();
  }

  private cancelUpdateRow(i: STData, edit: boolean): void {
    let item = this.blockDetails.find((x: any) => x.index == i['index']);

    if (
      !i['Level'] ||
      !i['FloorId'] ||
      !i['AreaId'] ||
      i['GeneralArea'] == undefined ||
      i['GeneralArea']?.toString() == '' ||
      i['PrivateArea'] == undefined ||
      i['PrivateArea']?.toString() == '' ||
      i['YardArea'] == undefined ||
      i['YardArea']?.toString() == '' ||
      i['DisposeTime'] == undefined ||
      i['DisposeTime'] == ''
    ) {
      this.blockDetails = this.blockDetails.filter((x: any) => x != item);
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

  async getDataArea() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `1=1`;
    paging.page_size = 0;
    paging.select = 'Id,Code,Name,FloorId';
    paging.order_by = 'Id Asc';

    const resp = await this.areaRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.area_data = resp.data;
    }
  }

  async getDataFloor() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = '1=1';
    paging.select = 'Id,Code,Name';
    paging.order_by = 'Id Asc';

    const resp = await this.floorRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.floor_data = resp.data;
    }
  }
}
