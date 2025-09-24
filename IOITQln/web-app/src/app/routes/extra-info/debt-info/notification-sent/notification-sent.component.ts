import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AddNotificationSentComponent } from './add-notification-sent/add-notification-sent.component';
import { ExtraViewNotiSendRepository } from 'src/app/infrastructure/repositories/ExtraViewNoti.repository';

@Component({
  selector: 'app-notification-sent',
  templateUrl: './notification-sent.component.html',
})
export class NotificationSentComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();
  
    data: any[] = [];
    loading = false;
  
    columns: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Mã định danh', index: 'NotificationTitle' },
        { title: 'Căn nhà', index: 'ContentNotification' },
        { title: 'Căn hộ', index: 'TypeNotification' },
        { title: 'Loại hỗ trợ', render: 'SendNotiByHeader', className: "text-center" },
        { title: 'Căn hộ', index: 'DateSend',type: 'date', dateFormat: 'dd/MM/yyyy' },
        {
          title: 'Chức năng',
          width: 100,
          className: 'text-center',
          buttons: [
            {
                icon: 'edit',
                iif: (i : any)=> !i.edit,
                click: (record : any) => this.addOrUpdate(record),
                tooltip : "Sửa"
              },
          ]
        }
    ];
  
    constructor(
        private drawerService: NzDrawerService,
        private modalSrv: NzModalService,
        private message: NzMessageService,
        private ExtraViewNotiSendRepository : ExtraViewNotiSendRepository
    ) { }
  
    ngOnInit(): void {
        this.getData();
    }
  
    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                break;
            case 'dblClick':
                break;
            case 'checkbox':
                break;
            case 'sort':
                this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
                break;
            default:
                break;
        }
    }
  
    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
    }
  
    searchData() {
        this.paging.page = 1;
    }
  
   async getData() {
      this.paging.query = '1=1';
      this.paging.order_by = 'CreatedAt Desc';
  
      if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
        if (this.query.txtSearch.trim() != '') this.paging.query += ` and Code.Contains("${this.query.txtSearch}")`;
      }
  
      try {
        this.loading = true;
        const resp = await this.ExtraViewNotiSendRepository.getByPage(this.paging);
  
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
  
    onBack() {
        window.history.back();
    }
    addOrUpdate(record?: any): void {
  
      const drawerRef = this.drawerService.create<AddNotificationSentComponent>({
          nzTitle: record ? `Sửa: ${record.Code}` : 'Thêm mới ',
          nzWidth: '55vw',
          nzContent: AddNotificationSentComponent,
          nzContentParams: {
              record,
          }
      });
  
      drawerRef.afterClose.subscribe((data: any) => {
          if (data) {
              let msg = data.Id ? `Sửa  ${data.Code} thành công!` : `Thêm  ${data.Code} thành công!`;
              this.message.success(msg);
              this.getData();
          }
      });
    }
  
    async delete(data: any) {
      const resp = await this.ExtraViewNotiSendRepository.delete(data);
      if (resp.meta?.error_code == 200) {
        this.message.create('success', `Xóa  thành công!`);
        this.getData();
      } else {
        this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
      }
    }
  }
  