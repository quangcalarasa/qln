import { Component, OnInit, ViewChild } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { EditContactManageComponent } from './edit/edit-contact-management.component';
import { ContactRepository } from 'src/app/infrastructure/repositories/contact.repositorty';

@Component({
  selector: 'Contact-Management',
  templateUrl: './Contact-management.component.html',
  styles: []
})


export class ContactManageComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any = [];
  loading = false;

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Tên khách hàng', index: 'FullName' },
    { title: 'Số điện thoại', index: 'Phone' },
    { title: 'Email', index: 'Email' },
    { title: 'Căn cước công dân', index: 'Code' },
    { title: 'Loại hỗ trợ', render: 'stateHeader', className: "text-center" },
    { title: 'Trạng thái hỗ trợ', render: 'supportTypeHeader', className: "text-center" },
    {
      title: 'Xử lý yêu cầu',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: (i: any) => !i.edit,
          click: (record: any) => this.addOrUpdate(record),
          tooltip: 'Đổi trạng thái'
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá?',
            okType: 'danger',
            icon: 'star'
          },
          click: (record: any) => this.delete(record),
          tooltip: 'Xóa'
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private ContactRepository: ContactRepository
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    console.log("hc ngu");
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
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and FullName.Contains("${this.query.txtSearch}")`;
    }

    try {
      this.loading = true;
      const resp = await this.ContactRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<EditContactManageComponent>({
      nzTitle: record ? `Sửa ` : 'Thêm mới ',
      // record.khoa_chinh
      nzWidth: '55vw',
      nzContent: EditContactManageComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa  thành công!` : `Thêm mới thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }


  async delete(data: any) {
    const resp = await this.ContactRepository.delete(data);
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

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }


}
