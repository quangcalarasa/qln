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
import { NzMessageService } from 'ng-zorro-antd/message';
import { AddDebtReminderComponent } from './add-debt-reminder/add-debt-reminder.component';
import { ExtraDebtReminderRepository } from 'src/app/infrastructure/repositories/extra-debt-reminder.repository';
@Component({
  selector: 'app-debt-reminder',
  templateUrl: './debt-reminder.component.html',
  styles: [
  ]
})
export class DebtReminderComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  title: string = "Quản lý nhắc nợ";

  typeComponent?: number = undefined;

  columns: STColumn[]=[ 
  { title: 'Stt', type: 'no', width: 40 },
  { title: 'Số nhắc nợ', index: 'DebtRemindNumber' ,width: 100 },
  { title: 'Ngày nhắc nợ', index: 'Date',type: 'date', },
  { title: 'Mã định danh', index: 'Code'},
  { title: 'Căn nhà', index: 'House'},
  { title: 'Địa chỉ', index: 'Address'},
  { title: 'Chủ hộ', index: 'Owner'},
  {
    title: 'Chức năng',
    width: 80,
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
          title: 'Bạn có chắc chắn muốn xoá loại nhà này?',
          okType: 'danger',
          icon: 'star'
        },
        click: record => this.delete(record)
      }
    ]
  }];

  constructor(
      private drawerService: NzDrawerService,
      private modalSrv: NzModalService,
      private extraDebtReminderRepository: ExtraDebtReminderRepository,
      private message: NzMessageService,
      private router: Router
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

  async getData(){
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.extraDebtReminderRepository.getByPage(this.paging);

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
  addOrUpdate(event?: any): void {

    const drawerRef = this.drawerService.create<AddDebtReminderComponent>({
        nzTitle: event ? `Sửa yêu cầu hỗ trợ: ${event.Name}` : 'Thêm mới yêu cầu hỗ trợ',
        nzWidth: '80vw',
        nzContent: AddDebtReminderComponent,
        nzContentParams: {
            event,
        }
    });

    drawerRef.afterClose.subscribe((data: any) => {
        if (data) {
            let msg = data.Id ? `Sửa yêu cầu hỗ trợ ${data.Name} thành công!` : `Thêm mới yêu cầu hỗ trợ ${data.Name} thành công!`;
            this.message.success(msg);
            // this.getData();
        }
    });
  }
  async delete(data: any) {
    const resp = await this.extraDebtReminderRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa loại nhà "${data.Name}" thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
}
