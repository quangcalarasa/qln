import { AddOrUpdateInstallmentComponent } from './../add-or-update/add-or-update-installment.component';
import { TdcInstallmentPriceTableComponent } from './../tdc-installment-price-table/tdc-installment-price-table.component';
import { PayTdcInstallmentPriceComponent } from './../pay-tdc-installment-price/pay-tdc-installment-price.component';
import { ImportExcelComponent } from './../import-excel/import-excel.component';
import { TdcInstallmentReportComponent } from './../tdc-installment-price-report/tdc-installment-report.component';
import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';

import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TypeContractTDC } from '../../../../shared/utils/enums';
import { AddendumTDCComponent } from '../addendum-tdc/addendum-tdc.component';

@Component({
  selector: 'app-list-addendum-tdc',
  templateUrl: './list-addendum-tdc.component.html',
  styles: []
})
export class ListAddendumTDCComponent implements OnInit, OnChanges {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;
  @Input() Id: string;
  @Input() change: boolean = false;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any;
  loading = false;

  columns: STColumn[] = [
    { title: 'STT', type: 'no', width: 40 },
    { title: 'Họ Tên Khách Hàng', index: 'CustomerName' },
    { title: 'Số Hợp Đồng', index: 'ContractNumber' },
    { title: 'Ngày Kí HĐ', index: 'DateNumber', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Dự Án', index: 'TdcProjectName' },
    { title: 'Lô', index: 'TdcLandName' },
    { title: 'Khối', index: 'TdcBlockHouseName' },
    { title: 'Lầu', index: 'Floor1' },
    { title: 'Tầng', index: 'TdcFloorName' },
    { title: 'Căn Số', index: 'TdcApartmentName' },
    { title: 'Lô Góc', index: 'Corner', type: 'yn', safeType: "safeHtml" },
    {
      title: 'Chức năng',
      width: 150,
      className: 'text-left',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.Addendum(record),
          tooltip: 'Sửa'
        },
        {
          icon: 'table',
          click: record => this.table(record),
          tooltip: 'Bảng chiết tính phụ lục'
        },
        {
          icon: 'dollar',
          click: record => this.Pay(record),
          tooltip: 'Thanh toán'
        },
        {
          icon: 'import',
          click: record => this.importExcel(record),
          tooltip: 'Thêm dữ liệu bảng chiết tính phụ lục'
        },
        {
          icon: 'export',
          click: record => this.exportExcel(record.Id),
          tooltip: 'Xuất dữ liệu bảng chiết tính  phụ lục'
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá phụ lục này không?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record),
          tooltip: 'Xóa  phụ lục'
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) {}

  ngOnInit(): void {}
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

  
  table(record?: any) {
    const drawerRef = this.drawerService.create<TdcInstallmentPriceTableComponent>({
      nzTitle: 'Bảng chiết tính',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: TdcInstallmentPriceTableComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  Pay(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<PayTdcInstallmentPriceComponent>({
      nzTitle: 'Thêm mới',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: PayTdcInstallmentPriceComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa  ${data.ContractNumber} thành công!` : `Thêm mới  ${data.ContractNumber} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  importExcel(record: any) {
    const drawerRef = this.drawerService.create<ImportExcelComponent>({
      nzTitle: 'Import dữ liệu từ Excel',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: ImportExcelComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  async exportExcel(record?: any) {
    let date = convertDate(new Date().toString());
    const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(record, date, false);
    if (resp.meta?.error_code == 200) {
      const respExport = await this.tDCInstallmentPriceRepository.ExportExcel(record, resp.data);
      if (resp.meta?.error_code != 200) this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `Type=${TypeContractTDC.EXTRA} AND ParentId=${this.Id}`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.tDCInstallmentPriceRepository.getByPage(this.paging);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        console.log(this.data);
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

  async delete(data: any) {
    const resp = await this.tDCInstallmentPriceRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa phụ lục hợp đồng ${data.ContractNumber} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  Addendum(data?: any) {
    this.modalSrv.create({
      nzTitle: `Phụ lục cho hợp đồng "${this.data.ContractNumber}"`,
      nzContent: AddendumTDCComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record: data,
        parent: data
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Sửa phụ lục hợp đồng  ${data.ContractNumber} thành công!`);
        this.getData();
      }
    });
  }
}
