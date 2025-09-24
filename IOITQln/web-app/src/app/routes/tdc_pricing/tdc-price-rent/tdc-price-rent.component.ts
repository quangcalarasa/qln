import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TdcPriceRentRepository } from 'src/app/infrastructure/repositories/tdcPriceRent.repository';
import { AddOrUpdatePriceRentComponent } from './add-or-update-price-rent/add-or-update-price-rent.component';
import { TdcPriceRentTableComponent } from './tdc-price-rent-table/tdc-price-rent-table.component';
import { PayTdcRentComponentComponent } from './pay-tdc-rent-component/pay-tdc-rent-component.component';
import { TdcPriceRentImportExComponent } from './tdc-price-rent-import-ex/tdc-price-rent-import-ex.component';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TdcPriceRentReportComponent } from './tdc-price-rent-report/tdc-price-rent-report.component';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';

@Component({
  selector: 'app-tdc-price-rent',
  templateUrl: './tdc-price-rent.component.html'
})
export class TdcPriceRentComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;
  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  project: any[] = [];
  selectedItems: any[] | undefined = undefined;

  loading = false;

  columns: STColumn[] = [
    { title: 'Check', index: 'Check', width: 40, type: 'checkbox' },
    { title: 'STT', type: 'no', width: 40 },
    { title: 'Số Hợp Đồng', index: 'Code' },
    { title: 'Họ Tên Khách Hàng', index: 'TdcCustomerName' },
    { title: 'Ngày Kí HĐ', index: 'Date', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Dự Án', index: 'TdcProjectName' },
    { title: 'Lô', index: 'TdcLandName' },
    { title: 'Khối', index: 'TdcBlockHouseName' },
    { title: 'Lầu', index: 'Floor1' },
    { title: 'Tầng', index: 'TdcFloorName' },
    { title: 'Căn Số', index: 'TdcApartmentName' },
    { title: 'Lô Góc', index: 'Corner', type: 'yn', safeType: "safeHtml" },
    {
      title: 'Chức năng',
      width: 120,
      className: 'text-left',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.addOrUpdate(record),
          tooltip: 'Sửa'
        },
        {
          icon: 'table',
          click: record => this.table(record),
          tooltip: 'Bảng chiết tính'
        },
        {
          icon: 'dollar',
          click: record => this.Pay(record),
          tooltip: 'Thanh toán'
        },
        {
          icon: 'import',
          click: record => this.importExcel(record),
          tooltip: 'Thêm dữ liệu bảng chiết tính'
        },
        {
          icon: 'export',
          click: record => this.exportExcel(record.Id),
          tooltip: 'Xuất dữ liệu bảng chiết tính'
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá hồ sơ này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record),
          tooltip: 'Xóa'
        }
      ]
    }
  ];
  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private tdcPriceRentRepository: TdcPriceRentRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private tdcProjectRepository: TDCProjectRepository
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getProjects();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        this.addOrUpdate(e.dblClick?.item);
        break;
      case 'checkbox':
        this.selectedItems = e.checkbox;
        break;
      default:
        break;
    }
  }

  reset(): void {
    this.query = new QueryModel();
    this.paging.page = 1;
    this.paging.page_size = 10;
    this.getData();
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  searchData() {
    this.paging.page = 1;
    this.getData();
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TdcProjectId=${this.query.type}`;
    }

    try {
      this.loading = true;
      const resp = await this.tdcPriceRentRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        this.paging.item_count = resp.metadata;
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

  addOrUpdate(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<AddOrUpdatePriceRentComponent>({
      nzTitle: record ? `Sửa hồ sơ: ${record.Code}` : 'Thêm mới hợp đồng',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdatePriceRentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hợp đồng ${data.Code} thành công!` : `Thêm mới hợp đồng ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  table(record?: any) {
    const drawerRef = this.drawerService.create<TdcPriceRentTableComponent>({
      nzTitle: 'Bảng chiết tính',
      // record.khoa_chinh
      nzWidth: '90vw',
      nzContent: TdcPriceRentTableComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  async exportExcel(record?: any) {
    let date = convertDate(new Date().toString());
    const resp = await this.tdcPriceRentRepository.getExcelTable(record, date);
    if (resp.meta?.error_code == 200) {

      const respExport = await this.tdcPriceRentRepository.ExportExcel(record, resp.data);
      if (resp.meta?.error_code != 200) this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  importExcel(record?: any) {
    const drawerRef = this.drawerService.create<TdcPriceRentImportExComponent>({
      nzTitle: 'Thêm dữ lệu',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: TdcPriceRentImportExComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Thêm dữ liệu thành công!` : ``;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  getReport() {
    if(!this.selectedItems || this.selectedItems.length === 0) {
      this.message.error('Vui lòng chọn ít nhất 1 hồ sơ giá thuê');
      return;
    }
    const drawerRef = this.drawerService.create<TdcPriceRentReportComponent>({
      nzTitle: 'Xuất báo cáo',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: TdcPriceRentReportComponent,
      nzPlacement: 'left',
      nzContentParams: {
        selectedItems: this.selectedItems
      }
    });
  }

  Pay(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<PayTdcRentComponentComponent>({
      nzTitle: 'Thanh toán',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: PayTdcRentComponentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Thanh toán thành công!` : ` Thanh toán thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.tdcPriceRentRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hợp đồng ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  onBack() {
    window.history.back();
  }

  //filter theo dự án
  async getProjects() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;

    const resp = await this.tdcProjectRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.project = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu dự án.'
      });
    }
  }

  findProject(id: number) {
    let tplGroup = this.project.find(x => x.Id == id);
    return tplGroup ? tplGroup.Name : undefined;
  }
}
