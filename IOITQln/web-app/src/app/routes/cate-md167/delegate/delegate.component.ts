import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateDelegateComponent } from './add-or-update/add-or-update.component';
import { Md167DelegateRepository } from 'src/app/infrastructure/repositories/md167delegate.repository';
import { TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';

@Component({
  selector: 'app-delegate',
  templateUrl: './delegate.component.html',
  styles: [
  ]
})
export class DelegateComponent implements OnInit {

  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  TypeReportApply = TypeReportApply;

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã đại diện', index: 'Code' },
    { title: 'Họ tên người đại diện/Cá nhân thuê', index: 'Name' },
    { title: 'Mã số thuế/mã số kinh doanh', index: 'ComTaxNumber' },
    { title: 'Chức vụ', index: 'ComPosition' },
    { title: 'Địa chỉ liên hệ', index: 'Address', },
    { title: 'Điện thoại liên hệ', index: 'PhoneNumber', },
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
            title: 'Bạn có chắc chắn muốn xoá đại diện tổ chức/ Cá nhân thuê này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private md167DelegateRepository: Md167DelegateRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
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

    try {
      this.loading = true;
      const resp = await this.md167DelegateRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdateDelegateComponent>({
      nzTitle: record ? `Sửa đại diện tổ chức/ Cá nhân thuê: "${record.Name}"` : 'Thêm mới đại diện tổ chức/ Cá nhân thuê',
      // record.khoa_chinh
      nzWidth: '80vw',
      nzContent: AddOrUpdateDelegateComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa đại diện tổ chức/ Cá nhân thuê "${data.Name}" thành công!` : `Thêm mới đại diện tổ chức/ Cá nhân thuê "${data.Name}" thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.md167DelegateRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa đại diện tổ chức/ Cá nhân thuê "${data.Name}" thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
}

