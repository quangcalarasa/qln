import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { STChange, STColumn } from '@delon/abc/st';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AddOrUpdateHandleComponent } from './add-or-update-handle/add-or-update-handle.component';
import { ExtraSupportHandleRepository } from 'src/app/infrastructure/repositories/extra-support-handle.repository'; 
import { NzModalService } from 'ng-zorro-antd/modal';
import { Router } from '@angular/router';
import { TypePersonSupportName } from 'src/app/shared/utils/consts';


@Component({
  selector: 'app-handle-support-requests',
  templateUrl: './handle-support-requests.component.html',
  styles: [
  ]
})
export class HandleSupportRequestsComponent implements OnInit {
  data: any;
  event: any;
  listData: any;
  loading = false;
  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data_type: any[] = [];
  typepersup_data: any[] = [];
  columns: STColumn[] = [];

  constructor(
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private router: Router,
    private extraSupportHandleRepository: ExtraSupportHandleRepository,
  ) {
    const curentUrl = this.router.url;
    this.columns =[
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Mã định danh', index: 'Code', className:'text-center' },
      { title: 'Căn nhà', index: 'House', className:'text-center' },
      { title: 'Căn hộ', index: 'Apartment', className:'text-center' },
      { title: 'Loại yêu cầu hỗ trợ', index: 'TypeSupport' },
      { title: 'Nội dung yêu cầu hỗ trợ', index: 'Content' },
      { title: 'Người yêu cầu', index: 'RequirePerson', className:'text-center'},
      { title: 'Thời gian yêu cầu hỗ trợ', index: 'ToDate',type: 'date', width: 120, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
      { title: 'Người tiếp nhận hỗ trợ', index: 'TypePersonSupportName' , className: 'text-center', type: 'enum', enum: TypePersonSupportName },
      {
          title: 'Chức năng',
          width: 100,
          className: 'text-center',
          buttons: [
            {
                icon: 'history',
                iif: i => !i.history,
            },
            {
                icon: 'search',
                iif: i => !i.search,
            },
            {
                icon: 'edit',
                iif: i => !i.edit,
                click: record => this.addOrUpdate(record)
            },
            {
                icon: 'user-add',
                iif: i => !i.useradd,
            },
            {
              icon: 'delete',
              type: 'del',
              pop: {
                title: 'Bạn có chắc chắn yêu cầu hỗ trợ này?',
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
      case 'sort':
        this.paging.order_by = e.sort?.value
          ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace('end', '')}`
          : new GetByPageModel().order_by;
        this.getData();
        break;
      default:
        break;
    }
  }

  addOrUpdate(record?: any): void {
    let typepersup_data = [...this.typepersup_data];

    const drawerRef = this.drawerService.create<AddOrUpdateHandleComponent>({
      nzTitle: record ? `Sửa yêu cầu xử lý hỗ trợ` : 'Thêm mới cầu yêu xử lý  hỗ trợ',
      nzWidth: '55vw',
      nzContent: AddOrUpdateHandleComponent,
      nzContentParams: {
        record,
        typepersup_data,
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa yêu cầu xử lý hỗ trợ thành công!` : `Thêm mới yêu cầu xử lý hỗ trợ ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }


  async delete(data: any) {
    const resp = await this.extraSupportHandleRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa yêu cầu hỗ trợ thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  async getData() {
  this.paging.query = '1=1';
  this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

  if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
    if (this.query.txtSearch.trim() != '') this.paging.query += ` and Code.Contains("${this.query.txtSearch}")`;
  }

  // if (this.query.type != undefined) {
  //   this.paging.query += ` and TypeBlockId=${this.query.type}`;
  // }

  try {
      this.loading = true;
      const resp = await this.extraSupportHandleRepository.getByPage(this.paging);

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

  onBack() {
    window.history.back();
  }
  exportReport() {
    // this.md167ContractRepository.exportReport(this.record.Id);
  }
}