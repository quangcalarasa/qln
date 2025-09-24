import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { TypeCoefficient } from 'src/app/shared/utils/consts';
import { DiscountCoefficientRepository } from 'src/app/infrastructure/repositories/discount-coefficient.repositories';
import { AddOrUpdateDiscountComponent } from './add-or-update-discount/add-or-update-discount.component';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { AccessKey, ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-discount-coefficient',
  templateUrl: './discount-coefficient.component.html',
  styles: []
})
export class DiscountCoefficientComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  unit_price_data: any[] = [];
  data: any[] = [];
  loading = false;
  role = this.commonService.CheckAccessKeyRole(AccessKey.DISCOUNT_COFFICIENT);
  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Ngày áp dụng', type: 'date', index: 'DoApply', className: 'text-center', dateFormat: 'dd/MM/yyyy' },
    { title: 'Giá trị', render: 'vClmn', className: 'text-center' },
    { title: 'Đơn vị tính', index: 'UnitPriceName', className: 'text-center' },
    { title: 'Ghi chú', index: 'Note', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit && this.role.Update,
          click: record => this.addOrUpdate(record)
        },
        {
          icon: 'delete',
          type: 'del',
          iif: i => this.role.Delete,
          pop: {
            title: 'Bạn có chắc chắn muốn xoá hệ số giảm giá này?',
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
    private discountCoefficientRepository: DiscountCoefficientRepository,
    private unitPriceRepository: UnitPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.getData();
    this.getUnitPrice();
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
    this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and ( Note.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.discountCoefficientRepository.getByPage(this.paging);

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
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<AddOrUpdateDiscountComponent>({
      nzTitle: record ? `Sửa hệ số giảm giá` : `Thêm mới  hệ số giảm giá`,
      // record.khoa_chinh
      nzWidth: '70vw',
      nzContent: AddOrUpdateDiscountComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        unit_price_data: this.unit_price_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hệ số giảm giá thành công!` : `Thêm mới hệ số giảm giá thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.discountCoefficientRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hệ số giảm giá thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  async getUnitPrice() {
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.unitPriceRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.unit_price_data = resp.data;
    }
  }
  onBack() {
    window.history.back();
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }
  
  import() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel Hệ số lương`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.NocDiscountCoefficient
        }
    });
  }

  async csvExport() {
    const resp = await this.discountCoefficientRepository.ExportExcel();
  }
}
