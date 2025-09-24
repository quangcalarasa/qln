import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { LogActionRepository } from 'src/app/infrastructure/repositories/log-action.repository';
import { TypeLogAction } from 'src/app/shared/utils/consts';
import { Router } from '@angular/router';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { AddUsageHistoryComponent } from './add-usage-history/add-usage-history.component';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TypeUsageStatus } from 'src/app/shared/utils/consts';
import { ExtraHistoryAndStatusRepository } from 'src/app/infrastructure/repositories/extrahistoryandstatus.repository';
import { FormBuilder, FormGroup } from '@angular/forms';
import { EntityStatus } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-usage-history',
  templateUrl: './usage-history.component.html',
  styles: [
  ]
})
export class UsageHistoryComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  validateForm!: FormGroup;
  csv: any[] = [];

  data: any[] = [];
  loading = false;

  title: string = "";

  typeComponent?: number = undefined;
  
  typeusagestatus_data:any[] =[];

  columns: STColumn[];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private extraHistoryAndStatusRepository: ExtraHistoryAndStatusRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router,
    
  ) {
    const curentUrl = this.router.url;
    this.columns = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Mã định danh', index: 'Code', className:'text-center' },
        { title: 'Căn nhà', index: 'House', className:'text-center' },
        { title: 'Căn hộ', index: 'Apartment', className:'text-center' },
        { title: 'Địa chỉ căn hộ', index: 'Address' },
        { title: 'Chủ căn hộ', index: 'Name' },
        { title: 'Diện tích sử dụng', index: 'Total' },
        { title: 'Điện thoại', index: 'Phone' },
        { title: 'Ngày cập nhật', index: 'ToDate',type: 'date', width: 120, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        { title: 'Trạng thái sử dụng', index: 'TypeUsageStatusName' , className: 'text-center', type: 'enum', enum: TypeUsageStatus },
      {
        title: 'Chức năng',
        width: 100,
        className: 'text-center',
        buttons: [
          {
            icon: 'lock',
            tooltip: 'Khóa lịch sử, tình trạng sử dụng nhà',
            iif: i => i.Status == 0,
            type: 'del',
            pop: {
              title: 'Bạn có chắc chắn muốn KHÓA lịch sử, tình trạng sử dụng nhà này?',
              okType: 'danger',
              icon: 'lock'
            },
            click: record => this.changeStatus(record, EntityStatus.LOCK)
          },
          {
            icon: 'unlock',
            tooltip: 'Mở khóa tài khoản',
            iif: i => i.Status == 98,
            type: 'del',
            pop: {
              title: 'Bạn có chắc chắn muốn MỞ KHÓA lịch sử, tình trạng sử dụng nhà này?',
              okType: 'danger',
              icon: 'unlock'
            },
            click: record => this.changeStatus(record, EntityStatus.INIT)
          },
          {
            icon: 'edit',
            iif: i => !i.edit,
            click: record => this.addOrUpdate(record)
          },
          {
            icon: 'delete',
            type: 'del',
            pop: {
              title: `Bạn có chắc chắn muốn xoá?`,
              okType: 'danger',
              icon: 'star'
            },
            click: record => this.delete(record)
          }
        ]
      }
    ]
  }
  

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';
    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + `or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.extraHistoryAndStatusRepository.getByPage(this.paging);

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

  async changeStatus(record: any, status: number) {
    let typeLock = status == 0 ? 'Mở khóa' : 'Khóa';
    const resp = await this.extraHistoryAndStatusRepository.changeStatus(record, status);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `${typeLock} lịch sử, trạng thái sử dụng nhà ${record.Code} thành công!`);
      this.getData();
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

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  resetPageSize() {
    this.paging.page_size = 20;
    this.getData();
  }

  addOrUpdate(record?: any): void {
    let typeusagestatus_data = [...this.typeusagestatus_data];

    const drawerRef = this.drawerService.create<AddUsageHistoryComponent>({
      nzTitle: record ? `Sửa lịch sử, trạng thái` : 'Thêm mới lịch sử, trạng thái',
      nzWidth: '55vw',
      nzContent: AddUsageHistoryComponent,
      nzContentParams: {
        record,
        typeusagestatus_data,
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa yêu lịch sử, trạng thái thành công!` : `Thêm mới lịch sử, trạng thái ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.extraHistoryAndStatusRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa  thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
}