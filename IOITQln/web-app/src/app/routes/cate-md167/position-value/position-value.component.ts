import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdatePositionValueComponent } from './add-or-update/add-or-update.component';
import { Md167PositionValueRepository } from 'src/app/infrastructure/repositories/md167positionvalue.repository';
import { TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';


@Component({
  selector: 'app-position-value',
  templateUrl: './position-value.component.html',
  styles: [
  ]
})
export class PositionValueComponent implements OnInit {

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
    { title: 'Ngày áp dụng', index: 'DoApply', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Vị trí 1', index: 'Position1', render: 'valueClmn1', className:'text-center' },
    { title: 'Vị trí 2', index: 'Position2', render: 'valueClmn2', className:'text-center' },
    { title: 'Vị trí 3', index: 'Position3', render: 'valueClmn3', className:'text-center' },
    { title: 'Vị trí 4', index: 'Position4', render: 'valueClmn4', className:'text-center' },
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
            title: 'Bạn có chắc chắn muốn xoá hệ số vị trí này?',
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
    private md167PositionValueRepository: Md167PositionValueRepository,
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
      const resp = await this.md167PositionValueRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdatePositionValueComponent>({
      nzTitle: record ? `Sửa hệ số vị trí ` : 'Thêm mới hệ số vị trí',
      // record.khoa_chinh
      nzWidth: '60vw',
      nzContent: AddOrUpdatePositionValueComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hệ số vị trí thành công!` : `Thêm mới hệ số vị trí thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.md167PositionValueRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hệ số vị trí thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
}

