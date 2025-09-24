import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Md167ManPaymentRepository } from 'src/app/infrastructure/repositories/md167-manage-payment.repository';
import { Router } from '@angular/router';
import { AddOrUpdateManagePaymentComponent } from './add-or-update/add-or-update.component';
@Component({
  selector: 'app-manage-payment',
  templateUrl: './manage-payment.component.html',
  styles: [
  ]
})
export class ManagePaymentComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;
  years: number[] = Array.from({ length: 100 }, (_, index) => 2000 + index);

  columns: STColumn[];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private md167ManPaymentRepository: Md167ManPaymentRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router
  ) {
    const curentUrl = this.router.url;
    this.columns = [
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Mã phiếu chi', index: 'Code', className: 'text-center', },
      { title: 'Ngày nộp tiền', index: 'Date', type: 'date', dateFormat: 'dd/MM/yyyy'  },
      { title: 'Năm nộp tiền', index: 'Year', className: 'text-center', },
      { title: 'Tổng tiền thuế', render: 'PaymentHeader', index: 'Payment', className: 'text-center', },
      { title: 'Ghi chú', index: 'Note', className: 'text-center' },
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
              title: `Bạn có chắc chắn muốn xoá phiếu chi này?`,
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

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        this.addOrUpdate(e.dblClick?.item);
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
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.md167ManPaymentRepository.getByPage(this.paging);

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
    let add;
    record ? add = true : add = false
    localStorage.setItem('add', add.toString())
    const drawerRef = this.drawerService.create<AddOrUpdateManagePaymentComponent>({
      nzTitle: record ? `Sửa phiếu chi : ${record.Code}` : `Thêm mới phiếu chi`,
      // record.khoa_chinh
      nzWidth: '90vw',
      nzContent: AddOrUpdateManagePaymentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa phiếu chi ${data.Code} thành công!` : `Thêm mới phiếu chi ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.md167ManPaymentRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa phiếu chi ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

}
