import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
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
import { AccessKey, ImportHistoryTypeEnum, TypeDecree } from 'src/app/shared/utils/enums';
import { ConversionRepository } from 'src/app/infrastructure/repositories/conversion.repository';
import { AddOrUpdateConversionComponent } from './add-or-update-conversion/add-or-update-conversion.component';
import { TypeCoefficient, TypeQD } from 'src/app/shared/utils/consts';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-conversion',
  templateUrl: './conversion.component.html',
  styles: []
})
export class ConversionComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  dataAll: any[] = [];
  loading = false;

  decree_typies: any[] = [];
  typeDecree: number = TypeDecree.THONGTU;
  role = this.commonService.CheckAccessKeyRole(AccessKey.CONVERSION);
  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Thông tư/Văn bản/Quyết định', index: 'TypeQD', className: 'text-center', type: 'enum', enum: TypeQD },
    { title: 'Tên hệ số', index: 'CoefficientName', className: 'text-center', type: 'enum', enum: TypeCoefficient },
    { title: 'Tên kí hiệu', index: 'Code', className: 'text-center' },
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
            title: 'Bạn có chắc chắn muốn xoá cấu hệ số quy đổi này?',
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
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private decreeRepository: DecreeRepository,
    private conversionRepository: ConversionRepository,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.getData();
    this.getDecree();
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
      case 'sort':
        this.paging.order_by = e.sort?.value
          ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace('end', '')}`
          : new GetByPageModel().order_by;
        this.getData();
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
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and Note.Contains("${this.query.txtSearch}")`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and ApartmentType=${this.query.type}`;
    }

    try {
      this.loading = true;
      const resp = await this.conversionRepository.getByPage(this.paging);

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
    let decree_typies = [...this.decree_typies];

    const drawerRef = this.drawerService.create<AddOrUpdateConversionComponent>({
      nzTitle: record ? `Sửa hệ số quy đổi ` : 'Thêm hệ số quy đổi ',
      nzWidth: '55vw',
      nzContent: AddOrUpdateConversionComponent,
      nzContentParams: {
        record,
        decree_typies
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hệ số quy đổi thành công!` : `Thêm mới hệ số quy đổi thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.conversionRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hệ số quy đổi thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  async getDecree() {
    this.paging.query = `TypeDecree=${this.typeDecree}`;
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or DecisionUnit.Contains("${this.query.txtSearch}") or Note.Contains("${this.query.txtSearch}"))`;
    }
    try {
      this.loading = true;
      const resp = await this.decreeRepository.getByPage(this.paging, this.typeDecree);

      if (resp.meta?.error_code == 200) {
        this.decree_typies = resp.data;
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu thông tư!!!!'
        });
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
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
            importHistoryType: ImportHistoryTypeEnum.NocConversion
        }
    });
  }

  async csvExport() {
    const resp = await this.conversionRepository.ExportExcel();
  }
}
