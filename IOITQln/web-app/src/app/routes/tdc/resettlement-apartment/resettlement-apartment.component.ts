import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateResettlementApartmentComponent } from './add-or-update/add-or-update-resettlement-apartment.component';
import { ResettlementApartmentRepository } from 'src/app/infrastructure/repositories/resettlement-apartment.repository';
import { read, utils, writeFile } from 'xlsx';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';

@Component({
  selector: 'app-resettlement-apartment',
  templateUrl: './resettlement-apartment.component.html'
})
export class ResettlementApartmentComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  csv: any[] = [];

  data: any[] = []; //
  loading = false;

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Chung cư', index: 'Name' , className: 'text-center'},
    { title: 'Địa chỉ chung cư', index: 'Address', className: 'text-center' },
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
            title: `Bạn có chắc chắn muốn xoá chung cư này?`,
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
    private resettlementapartmentRepository: ResettlementApartmentRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) {}

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

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  resetPageSize() {
    this.paging.page_size = 20;
    this.getData();
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Address.Contains("${this.query.txtSearch}"))`;
    }
    try {
      this.loading = true;
      const resp = await this.resettlementapartmentRepository.getByPage(this.paging);

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
    const drawerRef = this.drawerService.create<AddOrUpdateResettlementApartmentComponent>({
      nzTitle: record ? `Sửa chung cư: ${record.Name}` : `Thêm mới chung cư`,
      nzWidth: '85vw',
      nzContent: AddOrUpdateResettlementApartmentComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa chung cư ${data.Name} thành công!` : `Thêm mới chung cư ${data.Name} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.resettlementapartmentRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa chung cư thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
  
  async csvExport() {
    const resp = await this.resettlementapartmentRepository.ExportExcel();
  }

  import() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.TdcResettlement
        }
    });
  }
}
