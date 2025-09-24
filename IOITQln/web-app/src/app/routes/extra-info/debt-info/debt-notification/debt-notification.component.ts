import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { LogActionRepository } from 'src/app/infrastructure/repositories/log-action.repository';
import { TypeLogAction, TypeNotificationForm } from 'src/app/shared/utils/consts';
import { Router } from '@angular/router';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AddDebtNotificationComponent } from './add-debt-notification/add-debt-notification.component';
import { ExtraDebtNotificationRepository } from 'src/app/infrastructure/repositories/extradebtnotification.repository';
import { FormBuilder, FormGroup } from '@angular/forms';
import { newArray } from '@angular/compiler/src/util';

@Component({
  selector: 'app-debt-notification',
  templateUrl: './debt-notification.component.html',
  styles: [
  ]
})
export class DebtNotificationComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  validateForm!: FormGroup;
  csv: any[] = [];

  data: any[] = [];
  loading = false;

  title: string = "";
  input: any[] | undefined = undefined;
  typeComponent?: number = undefined;
  
  notificationform_data:any[] =[];
  columns: STColumn[];
  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private extraDebtNotificationRepository: ExtraDebtNotificationRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router,
    
  ) {
    const curentUrl = this.router.url;
    this.columns = [
      { title: 'Check', index: 'check', type:'checkbox' },
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Loại công nợ', index: 'DebtType', className:'text-center' },
      { title: 'Biểu mẫu thông báo', index: 'TypeNotificationForm' , className: 'text-center', type: 'enum', enum: TypeNotificationForm },
      { title: 'Tiêu đề', index: 'Title', className:'text-center' },
      { title: 'Content', index: 'Content' },
      { title: 'Nhóm đối tượng nhận thông báo', index: 'GroupNotification' },
      { title: 'Danh sách đối tượng nhận thông báo', index: 'ListNotification' },
      { title: 'Ngày giờ gửi', index: 'ToDate',type: 'date', width: 120, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        
      {
        title: 'Chức năng',
        width: 100,
        className: 'text-center',
        buttons: [
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
        this.paging.query += ` and (DebtType.Contains("${this.query.txtSearch}")` + `or DebtType.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.extraDebtNotificationRepository.getByPage(this.paging);

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

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  resetPageSize() {
    this.paging.page_size = 20;
    this.getData();
  }

  addOrUpdate(record?: any): void {
    let notificationform_data = [...this.notificationform_data];

    const drawerRef = this.drawerService.create<AddDebtNotificationComponent>({
      nzTitle: record ? `Sửa lịch sử, trạng thái` : 'Thêm mới lịch sử, trạng thái',
      nzWidth: '55vw',
      nzContent: AddDebtNotificationComponent,
      nzContentParams: {
        record,
        notificationform_data,
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa yêu cầu xử lý hỗ trợ thành công!` : `Thêm mới yêu cầu xử lý hỗ trợ thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.extraDebtNotificationRepository.delete(data);
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

  async CheckNotification(){
    console.log(this.input);
    
    if(!this.input || this.input.length === 0) {
      this.message.error('Vui lòng chọn ít nhất 1 công nợ');
      return;
    }
    let payload ={
      id: Array(),
      status: 10,
    }
    for(let i=0; i<this.input.length ;i++){
    payload.id.push(Number(this.input[i].Id));  
      
    } 
    const resp = await this.extraDebtNotificationRepository.changeStatus(payload);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Gửi thông báo thành công`);
      this.getData();
    }
  }

}