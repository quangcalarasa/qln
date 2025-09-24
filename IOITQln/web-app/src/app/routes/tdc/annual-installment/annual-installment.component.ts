import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { ImportHistoryTypeEnum, TypeDecree } from 'src/app/shared/utils/enums';
import { Router, ActivatedRoute } from '@angular/router';
import { AddOrUpdateAnnualInstallmentComponent } from './add-or-update/add-or-update.component';
// import { UnitPriceComponent } from '../../cate-common/unit-price/unit-price.component';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { AnnualInstallmentRepository } from 'src/app/infrastructure/repositories/annual-installment.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
  selector: 'app-annual-installment',
  templateUrl: './annual-installment.component.html',
  styles: []
})
export class AnnualInstallmentComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  unit_price_data: any[] = [];
  data: any[] = [];
  data2: any[] = [];
  loading = false;

  titleString?: string = 'Lãi suất trả góp hàng năm';

  columns: STColumn[];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private annualInstallmentRepository: AnnualInstallmentRepository,
    private unitPriceRepository: UnitPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router
  ) {
    const curentUrl = this.router.url;

    this.columns = [
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Ngày áp dụng', index: 'DoApply', type: 'date', dateFormat: 'dd/MM/yyyy' },
      { title: 'Giá trị', render: 'valueClmn' },
      { title: 'Đơn vị tính', index: 'UnitPriceName' },
      { title: 'Diễn giải', index: 'Note' },
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
              title: `Bạn có chắc chắn muốn xoá ${this.titleString} này?`,
              okType: 'danger',
              icon: 'star'
            },
            click: record => this.delete(record)
          }
        ]
      }
    ];
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
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
    this.getData();
  }

  searchData() {
    this.paging.page = 1;
    this.getData();
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'DoApply Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and ( Note.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.annualInstallmentRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdateAnnualInstallmentComponent>({
      nzTitle: record ? `Sửa ${this.titleString} : ${record.Id}` : `Thêm mới ${this.titleString}`,
      // record.khoa_chinh
      nzWidth: '60vw',
      nzContent: AddOrUpdateAnnualInstallmentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        unit_price_data: this.unit_price_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa thành công!` : `Thêm mới thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.annualInstallmentRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa thành công!`);
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

  async csvExport() {
    const resp = await this.annualInstallmentRepository.ExportExcel();
  }

  import() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.TdcInstallmentRate
        }
    });
  }
  onBack() {
    window.history.back();
  }
}
