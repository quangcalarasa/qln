import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalRef, NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import { AddOrUpdateUserComponent } from './add-or-update/add-or-update-user.component';
import gatewayConfig from 'src/app/infrastructure/http/api-gateway-config';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { ChangePassUserComponent } from './change-pass/change-pass-user.component';
import { EntityStatus } from 'src/app/shared/utils/enums';
import { UserService } from 'src/app/core/services/user.service';
import { RoleRepository } from 'src/app/infrastructure/repositories/role.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html'
})
export class UserComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  statusId: any;
  data: any[] = [];
  dataAll: any[] = [];
  loading = false;
  selectAll?: any[] = [];
  srcImg = this.userService.getLoggedUser()['BaseUrlImg'];
  rolesInput: any[] = [];
  statusOptions = [
    {label: 'Hoạt động', value: '1'},
    {label: 'Khóa', value: '98'}
  ];

  districts: any[] = [];
  wards: any[] = [];
  
  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40, },
    { title: 'Avatar', render: 'ava-column', type: 'img', width: 60 },
    { title: 'Tên người dùng', index: 'FullName', sort: true },
    { title: 'Tài khoản', index: 'UserName', sort: true },
    { title: 'Số điện thoại', index: 'Phone' },
    { title: 'Email', index: 'Email' },
    { title: 'Địa chỉ', index: 'Address' },
    { title: 'Thời gian tạo', index: 'CreatedAt', type: 'date', width: 120, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm' },
    { title: 'Thời gian sửa', index: 'UpdatedAt', type: 'date', width: 120, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm' },
    {
      title: 'Chức năng',
      width: 140,
      className: 'text-center',
      buttons: [
        {
          icon: 'lock',
          tooltip: 'Khóa tài khoản',
          iif: i => i.Status == 1,
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn KHÓA tài khoản này?',
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
            title: 'Bạn có chắc chắn muốn MỞ KHÓA tài khoản này?',
            okType: 'danger',
            icon: 'unlock'
          },
          click: record => this.changeStatus(record, EntityStatus.NORMAL)
        },
        {
          icon: 'key',
          tooltip: 'Đổi mật khẩu',
          iif: i => !i.edit,
          click: record => this.adminChangePass(record)
        },
        {
          icon: 'edit',
          iif: i => !i.edit,
          tooltip: 'Sửa',
          click: record => this.addOrUpdate(record)
        },
        {
          icon: 'delete',
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
    private modalSrv: NzModalService,
    private userRepository: UserRepository,
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private nzNotificationService: NzNotificationService,
    private userService: UserService,
    private roleRepository: RoleRepository,
    private districtRepository: DistrictRepository,
    private wardRepository: WardRepository
  ) { }

  ngOnInit(): void {
    this.getData();
    this.getListRole();
    this.getDistricts();
    this.getWards();
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
    // this.query.status
    this.paging.page = 1;
    this.getData();
    this.statusId = this.query.status;
    this.getDataStatus();
  }

  async getData(param?: any) {
    this.paging.query = '1=1';

    if(this.query.status != undefined) {
      this.paging.query += ` and Status=${this.query.status}`;
    }

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (UserName.Contains("${this.query.txtSearch}") or FullName.Contains("${this.query.txtSearch}") or Phone.Contains("${this.query.txtSearch}") or Email.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;

      const resp = await this.userRepository.getByPage(this.paging, this.query.type);

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
    const drawerRef = this.drawerService.create<AddOrUpdateUserComponent>({
      nzTitle: record ? `Sửa thông tin tài khoản ${record.UserName}` : 'Thêm mới tài khoản',
      nzWidth: '65vw',
      nzContent: AddOrUpdateUserComponent,
      nzContentParams: {
        record,
        srcImg: this.srcImg,
        districts: this.districts,
        wards: this.wards
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa tài khoản ${data.UserName} thành công!` : `Thêm mới tài khoản ${data.UserName} thành công!`;
        this.message.create('success', msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.userRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa tài khoản ${data.FullName} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  deletes() {
    console.log(this.selectAll);
    if (this.selectAll?.length) {
    }
    this.nzNotificationService.warning('Thông báo', 'Chưa có api xóa nhiều!');
  }

  onBack() {
    window.history.back();
  }

  async changeStatus(record: any, status: number) {
    let typeLock = status == 1 ? 'Mở khóa' : 'Khóa';
    const resp = await this.userRepository.changeStatus(record, status);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `${typeLock} tài khoản ${record.FullName} thành công!`);
      this.getData();
    }
  }

  adminChangePass(record: any) {
    this.modalSrv.create({
      nzTitle: `Thay đổi mật khẩu tài khoản "${record.FullName}"`,
      nzContent: ChangePassUserComponent,
      nzComponentParams: {
        record: record
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thay đổi mật khẩu tài khoản ${record.FullName} thành công!`);
      }
    });
  }

  async getDataStatus(){
    // const resp = await this.userRepository.getByStatus(this.statusId);
  }

  async getListRole() {
    const paging: GetByPageModel = new GetByPageModel();
    paging.select = 'Id,Name,Note';
    paging.order_by = 'Name Asc';
    paging.page_size = -1;

    const resp = await this.roleRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.rolesInput = resp.data;
      // this.rolesInput = resp.data.map((x: any) => {
      //   return { RoleId: x.Id, RoleName: x.Name, FullName: x.Name + (x.Note ? ` (${x.Note})` : ``) };
      // });
    }
  }

  async getDistricts() {
    const paging: GetByPageModel = new GetByPageModel();
    paging.select = 'Id,Name';
    paging.order_by = 'Name Asc';
    paging.query = "ProvinceId=2";
    paging.page_size = -1;

    const resp = await this.districtRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.districts = resp.data;
    }
  }

  async getWards() {
    const paging: GetByPageModel = new GetByPageModel();
    paging.select = 'Id,Name,DistrictId';
    paging.order_by = 'Name Asc';
    paging.query = "ProvinceId=2";
    paging.page_size = -1;

    const resp = await this.wardRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.wards = resp.data;
    }
  }

  export() {
    this.userRepository.ExportExcel();
  }
}
