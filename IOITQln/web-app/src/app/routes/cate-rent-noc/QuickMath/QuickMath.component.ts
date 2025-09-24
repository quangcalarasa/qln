import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { AccessKey, ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { QuickmathRepository } from 'src/app/infrastructure/repositories/qiuckmath.repository';
import { AddQuickMathComponent } from './AddQiuckMath/AddQuickMath.component';
import { LogsQuickMathComponent } from './LogsQuickMath/LogsQuickMath.component';

@Component({
  selector: 'app-QuickMath',
  templateUrl: './QuickMath.component.html',
})

export class QuickMathComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  unit_price_data: any[] = [];
  data: any[] = [];
  loading = false;
  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Ngày thay đổi', type: 'date', index: 'CreatedAt', className: 'text-center', dateFormat: 'dd/MM/yyyy' },
    { title: 'Ngày áp dụng', type: 'date', index: 'DoApply', className: 'text-center', dateFormat: 'dd/MM/yyyy' },
    { title: 'Giá trị', index: 'Value', className: 'text-center' },
    { title: 'Hệ số thay đổi', index: 'TypeValue', className: 'text-center' },
    { renderTitle: 'StatusHeader', render: 'Status', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'eye',
          iif: i => !i.edit,
          click: record => this.getLogs(record)
        },
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private unitPriceRepository: UnitPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private QuickmathRepository : QuickmathRepository
  ) {}

  ngOnInit(): void {
    this.getData();
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

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and ( Note.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.QuickmathRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddQuickMathComponent>({
      nzTitle:`Công cụ tính giá thuê nhanh`,
      nzWidth: '70vw',
      nzContent: AddQuickMathComponent,
      nzPlacement: 'left',
      nzContentParams: {
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg =`Gửi yêu cầu tính giá nhanh thành công!!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  getLogs(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<LogsQuickMathComponent>({
      nzTitle:`Lịch sử tính toán`,
      nzWidth: '70vw',
      nzContent: LogsQuickMathComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
      }
    });
  }

  onBack() {
    window.history.back();
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }
}
