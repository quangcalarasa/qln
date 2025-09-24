import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import { AddOrUpdateCustomerComponent } from './add-or-update/add-or-update-customer.component';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { TypeSex } from 'src/app/shared/utils/consts';
import { UserService } from 'src/app/core/services/user.service';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-cate-customer',
  templateUrl: './customer.component.html'
})
export class CustomerComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  srcImg = this.userService.getLoggedUser()['BaseUrlImg'];

  role = this.commonService.CheckAccessKeyRole(AccessKey.CUSTOMER_MANAGEMENT);

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40, },
    { title: 'Avatar', render: 'ava-column', type: 'img', width: 60 },
    { title: 'CCCD/CMND', index: 'Code' },
    { title: 'Tên người dùng', index: 'FullName' },
    { title: 'Ngày sinh', index: 'Dob', type: 'date', className: 'text-center', dateFormat: 'dd/MM/yyyy' },
    { title: 'Giới tính', index: 'Sex', type: 'enum', enum: TypeSex, className: 'text-center' },
    { title: 'Số điện thoại', index: 'Phone' },
    { title: 'Email', index: 'Email' },
    { title: 'Địa chỉ', index: 'Address' },
    {
      title: 'Chức năng',
      width: 140,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit && this.role.Update,
          tooltip: 'Sửa',
          click: record => this.addOrUpdate(record)
        },
        {
          icon: 'delete',
          iif: i =>this.role.Delete,
          tooltip: 'Xóa',
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
    private customerRepository: CustomerRepository,
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private userService: UserService,
    private commonService: CommonService,
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
      case 'sort':
        this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
        this.getData();
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

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or FullName.Contains("${this.query.txtSearch}") or Phone.Contains("${this.query.txtSearch}") or Email.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;

      const resp = await this.customerRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data.map((dataItem: any, index: number) => {
          if (dataItem.Avatar) dataItem.AvatarClone = this.srcImg.concat(dataItem.Avatar);
          return dataItem;
        });
        this.paging.item_count = resp.metadata;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  addOrUpdate(record?: any): void {
    const drawerRef = this.drawerService.create<AddOrUpdateCustomerComponent>({
      nzTitle: record ? `Sửa thông tin khách hàng ${record.FullName}` : 'Thêm mới khách hàng',
      nzWidth: '65vw',
      nzContent: AddOrUpdateCustomerComponent,
      nzContentParams: {
        record,
        srcImg: this.srcImg
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa khách hàng ${data.FullName} thành công!` : `Thêm mới khách hàng ${data.FullName} thành công!`;
        this.message.create('success', msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.customerRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa khách hàng ${data.FullName} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
}
