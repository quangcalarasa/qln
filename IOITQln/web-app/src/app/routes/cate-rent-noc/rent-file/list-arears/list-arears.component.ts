import { DebtComponent } from './../debt/debt.component';

import { Component, Input, OnInit, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { RentFileBCTRepository } from 'src/app/infrastructure/repositories/rent-File-BCT.repository';

import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { ArrearsComponent } from '../arrears/arrears.component';

@Component({
  selector: 'app-list-arears',
  templateUrl: './list-arears.component.html',
  styles: []
})
export class ListArearsComponent implements OnInit, OnChanges {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  @Input() RentFileId: NzSafeAny;
  @Input() change1: boolean = false;
  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any;
  loading = false;

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã hợp đồng', index: 'CodeHd' },
    { title: 'Mã định danh', index: 'Code' },
    { title: 'Ngày bắt đầu', index: 'DateStart', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Ngày kết thúc', index: 'DateEnd', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Diện tích truy thu', render: 'aClmn', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'table',
          iif: i => !i.edit,
          click: record => this.getWorkSheet(record),
          tooltip: 'Bảng chiết tính'
        },

        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá phiếu truy thu này không?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private rentFileBCTRepository: RentFileBCTRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void { }

  ngOnChanges(changes: SimpleChanges): void {
    this.getData();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        break;
      case 'checkbox':
        break;
      default:
        break;
    }
  }

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `TypeBCT=${2} AND RentFileId.ToString().Contains("${this.RentFileId}")`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.rentFileBCTRepository.getByPage(this.paging);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu.'
        });
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  getWorkSheet(record?: any): void {
    const drawerRef = this.drawerService.create<ArrearsComponent>({
      nzTitle: 'Bảng chiết tính',
      nzWidth: '100vw',
      nzContent: ArrearsComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        RentFileId: this.RentFileId
      }
    });
  }

  debt(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<DebtComponent>({
      nzTitle: 'Công nợ',
      nzWidth: '85vw',
      nzContent: DebtComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  async delete(data: any) {
    const resp = await this.rentFileBCTRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa phiếu truy thu ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  Arrears(data?: any) {
    this.modalSrv.create({
      nzTitle: `Phiếu truy thu "${this.data.Code}"`,
      nzContent: ArrearsComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        data: this.data
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thêm mới truy thu cho hợp đồng ${data.Code} thành công!`);
        this.getData();
      }
    });
  }
}
