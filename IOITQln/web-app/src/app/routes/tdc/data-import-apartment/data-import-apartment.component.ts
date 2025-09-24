import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { ImportHistoryRepository } from 'src/app/infrastructure/repositories/import-history.repository';
import { DataImportApartmentRepository } from 'src/app/infrastructure/repositories/data-import-apartment.repository';

@Component({
  selector: 'app-data-import-apartment',
  templateUrl: './data-import-apartment.component.html',
  styles: [
  ]
})
export class DataImportApartmentComponent implements OnInit {
  data: any[] = [];
  loading = false;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  TypeReception = {
    Received: 1,
    ReceivedYet: 2,
    NotReceived: 3,
  };
  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', render: 'no-column', width: 60, className: 'text-center' },
    { title: 'Người thực hiện', index: 'CreatedBy', width: 200 },
    { title: 'Thời gian thực hiện', index: 'CreatedAt', width: 150, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm', type: 'date' },
    {
      title: 'File import',
      width: '150px',
      className: 'text-center',
      buttons: [
        {
          icon: 'download',
          click: record => this.downloadFileExcelHistory(record.Id)
        }
      ]
    },
    {
      title: 'Chi tiết',
      width: '150px',
      className: 'text-center',
      buttons: [
        {
          icon: 'eye',
          click: record => { this.dataImportDetail = record.Data }
        }
      ]
    }
  ];

  dataImport: any[] = [];
  dataImportDetail: any[] = [];
  constructor(private uploadRepository: UploadRepository, 
    private importHistoryRepository: ImportHistoryRepository, private dataImportApartmentRepository: DataImportApartmentRepository) { }

  ngOnInit(): void {
    this.getData();
  }

  TypeReceptionName(typeReception: number): string {
    switch (typeReception) {
      case this.TypeReception.Received:
        return 'Đã tiếp nhận';
      case this.TypeReception.ReceivedYet:
        return 'Chưa tiếp nhận';
      case this.TypeReception.NotReceived:
        return 'Không tiếp nhận';
      default:
        return 'lỗi';
    }
  }
  async getData() {
    this.paging.page_size = 10;
    const resp = await this.importHistoryRepository.getByPage(this.paging, ImportHistoryTypeEnum.Tdc_ApartmentDataImport);
    if (resp.meta?.error_code == 200) {
      this.dataImport = resp.data;
      this.paging.item_count = resp.metadata;
    }
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        break;
      case 'checkbox':
        break;
      case 'sort':
        this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
        this.getData();
        break;
      default:
        break;
    }
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.data = [];
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    this.loading = true;
    const resp_ren = await this.dataImportApartmentRepository.importDataExcel(formData);
    this.data = resp_ren.data;
    this.getData();
    this.loading = false;
  }


  downloadTemplate() {
    this.uploadRepository.downloadFileExcelTemplate(ImportHistoryTypeEnum.Tdc_ApartmentDataImport);
  }

  downloadFileExcelHistory(id: number) {
    this.uploadRepository.downloadFileExcelHistory(id);
  }

}
