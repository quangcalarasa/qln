import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TdcPriceOneSellRepository } from 'src/app/infrastructure/repositories/tdcPriceOneSell.repository'; 
import { AddOrUpdateTdcPriceOneSellComponent } from './add-or-update-tdc-price-one-sell/add-or-update-tdc-price-one-sell.component';
import { TdcPriceOneSellTableComponent } from './tdc-price-one-sell-table/tdc-price-one-sell-table.component';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
@Component({
  selector: 'app-tdc-price-one-sell',
  templateUrl: './tdc-price-one-sell.component.html',
  styles: [
  ]
})
export class TdcPriceOneSellComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;
  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];

  input: any[] | undefined = undefined;

  project: any[] = [];

  loading = false;

  columns: STColumn[] = [
    { title: 'Check', index: 'check', type:'checkbox' },
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
      width: 150,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.addOrUpdate(record),
          tooltip: 'Sửa hồ sơ',
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá hồ sơ này?',
            okType: 'danger',
            icon: 'star',
          },
          click: record => this.delete(record),
          tooltip:'Xóa hồ sơ'
        },
        // {
        //   icon:'import'

        // },
      ]
    }
  ];
  constructor(private fb: FormBuilder,
    private modalSrv: NzModalService,
    private TdcPriceOneSellRepository: TdcPriceOneSellRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    
    private tdcProjectRepository:TDCProjectRepository,
    ) {
   }

   ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getProject();
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
      const resp = await this.TdcPriceOneSellRepository.getByPage(this.paging);

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
        this.input = e.checkbox;
        break;
      default:
        break;
    }
  }
  reset(): void {
    this.query = new QueryModel();
    this.paging.page = 1;
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
    const drawerRef = this.drawerService.create<AddOrUpdateTdcPriceOneSellComponent>({
      nzTitle: record ? `Sửa hồ sơ: ${record.Code}` : `Thêm mới hồ sơ:`,
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateTdcPriceOneSellComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa mã hồ sơ ${data.Code} thành công!` : `Thêm mới mã hồ sơ thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }
  Table(){
    if(!this.input || this.input.length === 0) {
      this.message.error('Vui lòng chọn ít nhất 1 hồ sơ giá bán 1 lần');
      return;
    }
    const drawerRef = this.drawerService.create<TdcPriceOneSellTableComponent>({
      nzTitle: 'Biểu mẫu Báo cáo tổng hợp bán một lần',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: TdcPriceOneSellTableComponent,
      nzPlacement: 'left',
      nzContentParams: {
        input: this.input
      }
    });
  }

  async delete(data: any) {
    const resp = await this.TdcPriceOneSellRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa mã hồ sơ ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  async getProject() {
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
  
  onBack() {
    window.history.back();
  }

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }
}

