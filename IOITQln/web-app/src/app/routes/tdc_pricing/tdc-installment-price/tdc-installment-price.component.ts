import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateInstallmentComponent } from './add-or-update/add-or-update-installment.component';
import { TDCInstallmentPriceRepository } from 'src/app/infrastructure/repositories/tdc-installment.repository';
import { TdcInstallmentPriceTableComponent } from './tdc-installment-price-table/tdc-installment-price-table.component';
import { PayTdcInstallmentPriceComponent } from './pay-tdc-installment-price/pay-tdc-installment-price.component';
import { ImportExcelComponent } from './import-excel/import-excel.component';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TdcInstallmentReportComponent } from './tdc-installment-price-report/tdc-installment-report.component';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';

@Component({
  selector: 'app-tdc-installment-price',
  templateUrl: './tdc-installment-price.component.html',
  styles: []
})
export class TdcInstallmentPriceComponent implements OnInit {
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
          icon: 'file-excel',
          click: record => this.exportExReport2(record.Id),
          tooltip: 'Xuất dữ liệu báo cáo mẫu 2'
        },
        {
          icon: 'file-done',
          click: record => this.tableReport(record),
          tooltip: 'Xuất dữ liệu báo cáo mẫu 3'
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
    private tDCInstallmentPriceRepository: TDCInstallmentPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private tdcProjectRepository: TDCProjectRepository
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getProjects();
  }
  async getData() {
    this.paging.query = `Type!=2`;
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query +=
          ` and (ContractNumber.Contains("${this.query.txtSearch}")` + ` or ContractNumber.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TdcProjectId=${this.query.type}`;
    }

    try {
      this.loading = true;
      const resp = await this.tDCInstallmentPriceRepository.getByPage(this.paging);

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

  async tableReport(record?: any) {
    let date : any;
    let newDate=new Date();
    let defaultDate=new Date(record.FirstPayDate);
    defaultDate.setFullYear(newDate.getFullYear());
    if(defaultDate<newDate)
    defaultDate.setFullYear(defaultDate.getFullYear()+1);
    date=convertDate(defaultDate.toString())
    const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(record.Id, date, false);
    if (resp.meta?.error_code == 200) {
      const resp1 = await this.tDCInstallmentPriceRepository.GetReport(record.Id, resp.data);
      if (resp1.meta?.error_code == 200) {
        const resps = await this.tDCInstallmentPriceRepository.ExportReport3(resp1.data);
      }
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
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

  async exportExReport2(record?: any) {
    let date = convertDate(new Date().toString());
    const resp = await this.tDCInstallmentPriceRepository.getWorkSheet(record, date, false);
    if (resp.meta?.error_code == 200) {
      const resp1 = await this.tDCInstallmentPriceRepository.GetReportNo2(record, resp.data);
      if (resp1.meta?.error_code == 200) {
        const reps = await this.tDCInstallmentPriceRepository.ExportReportNo2(resp1.data);
      }
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
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
      nzTitle: 'Thanh toán',
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
        let msg = `Thanh toán thành công!`;
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

  searchData() {
    this.paging.page = 1;
    this.getData();
  }
  addOrUpdate(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<AddOrUpdateInstallmentComponent>({
      nzTitle: record ? `Sửa Hồ Sơ: ${record.ContractNumber}` : 'Thêm mới hồ sơ',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateInstallmentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa Hồ Sơ: ${data.ContractNumber} thành công!` : `Thêm mới Hồ Sơ: ${data.ContractNumber} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  getReport() {
    if(!this.selectedItems || this.selectedItems.length === 0) {
      this.message.error('Vui lòng chọn ít nhất 1 hồ sơ bán trả góp');
      return;
    }
    const drawerRef = this.drawerService.create<TdcInstallmentReportComponent>({
      nzTitle: 'Mẫu báo cáo số 1',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: TdcInstallmentReportComponent,
      nzPlacement: 'left',
      nzContentParams: {
        selectedItems: this.selectedItems
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Thêm Dữ liệu thành công!` : ``;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.tDCInstallmentPriceRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa Hồ Sơ: ${data.ContractNumber} thành công!`);
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
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }
}
