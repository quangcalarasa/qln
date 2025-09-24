import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalRef, NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { RoleRepository } from 'src/app/infrastructure/repositories/role.repository';
import { Location } from '@angular/common';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import { AddOrUpdateRoleComponent } from './add-or-update/add-or-update-role.component';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html'
})
export class RoleComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  dataAll: any[] = [];
  loading = false;
  selectAll?: any[] = [];

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã quyền', index: 'Code' },
    { title: 'Tên quyền', index: 'Name', width: '25%' },
    { title: 'Mô tả', index: 'Note' },
    { title: 'Thời gian tạo', index: 'CreatedAt', type: 'date', width: 120, dateFormat: 'dd/MM/yyyy HH:mm' },
    { title: 'Thời gian sửa', index: 'UpdatedAt', type: 'date', width: 120, dateFormat: 'dd/MM/yyyy HH:mm' },
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
            title: 'Bạn có chắc chắn muốn xoá?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];

  constructor(
    private modalSrv: NzModalService,
    private roleRepository: RoleRepository,
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private nzNotificationService: NzNotificationService
  ) { }

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
        this.selectAll = e.checkbox;
        break;
    }
  }

  reset(): void {
    this.query = new QueryModel();
    this.paging.page = 1;
    this.getData();
  }

  searchData() {
    // this.query.status
    this.paging.page = 1;
    this.getData();
  }

  async getData(param?: any) {
    this.paging.query = '1=1';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or Name.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.roleRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdateRoleComponent>({
      nzTitle: record ? `Sửa thông tin quyền ${record.Name}` : 'Thêm mới quyền',
      nzWidth: '90vw',
      nzContent: AddOrUpdateRoleComponent,
      nzContentParams: { record, listFunctionInput: [] }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa thông tin quyền ${data.Name} thành công!` : `Thêm mới quyền ${data.Name} thành công!`;
        this.message.create('success', msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.roleRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa quyền ${data.Name} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  deletes() {
    if (this.selectAll?.length) {
    }
    this.nzNotificationService.warning('Thông báo', 'Chưa có api xóa nhiều!');
  }

  onBack() {
    window.history.back();
  }

  
}
