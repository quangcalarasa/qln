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
import { TypeDecree } from 'src/app/shared/utils/enums';
import { Router, ActivatedRoute } from '@angular/router';
import { AddOrUpdateMd167ProfitValueComponent } from './add-or-update/add-or-update.component';
// import { UnitPriceComponent } from '../../cate-common/unit-price/unit-price.component';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { Md167ProfitValueRepository } from 'src/app/infrastructure/repositories/md167-profit-value.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { ImportExcelMd167PVComponent } from './import-excel/import-excel.component';

@Component({
  selector: 'app-profit-value',
  templateUrl: './profit-value.component.html',
  styles: [
  ]
})
export class Md167ProfitValueComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  unit_price_data: any[] = [];
  data: any[] = [];
  data2: any[] = [];
  loading = false;

  titleString?: string = 'Hệ số lãi phạt thuê';

  columns: STColumn[];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private md167ProfitValueRepository: Md167ProfitValueRepository,
    private unitPriceRepository: UnitPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router
  ) {
    const curentUrl = this.router.url;

    this.columns = [
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Ngày áp dụng', index: 'DoApply', type: 'date', dateFormat: 'dd/MM/yyyy' },
      { title: 'Giá trị', index: 'Value', render: 'valueClmn', className:'text-center' },
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
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and (Note.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.md167ProfitValueRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdateMd167ProfitValueComponent>({
      nzTitle: record ? `Sửa ${this.titleString} : ${record.Id}` : `Thêm mới ${this.titleString}`,
      // record.khoa_chinh
      nzWidth: '60vw',
      nzContent: AddOrUpdateMd167ProfitValueComponent,
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
    const resp = await this.md167ProfitValueRepository.delete(data);
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
  onBack() {
    window.history.back();
  }

  async csvExport() {
    const resp = await this.md167ProfitValueRepository.ExportExcel();
  }

  async csvImport(record?: any) {

    const drawerRef = this.drawerService.create<ImportExcelMd167PVComponent>({
      nzTitle: 'Import dữ liệu',
      nzWidth: '100vw',
      nzContent: ImportExcelMd167PVComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Thêm dữ liệu thành công!` : ``;
        this.message.success(msg);
        this.getData();
      }
    });
  }
}
