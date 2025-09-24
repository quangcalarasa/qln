import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import { GetByPageMd167HouseModel } from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddorupdateHouseComponent } from './addorupdate/addorupdate.component';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import { TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { Md167HouseFileComponent } from './md167-house-file/md167-house-file.component';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';


@Component({
  selector: 'app-house',
  templateUrl: './house.component.html',
  styles: [
  ]
})
export class HouseComponent implements OnInit {

  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageMd167HouseModel = new GetByPageMd167HouseModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  TypeReportApply = TypeReportApply;

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã nhà', index: 'Code' },
    { title: 'Số nhà', index: 'HouseNumber', width: 60 },
    { title: 'Đường', index: 'LaneName' },
    { title: 'Phường', index: 'WardName' },
    { title: 'Quận/Huyện', index: 'DistrictName', },
    { title: 'Tỉnh/TP', index: 'ProvinceName', },
    { title: 'Loại nhà', index: 'TypeHouseName', },
    { title: 'Hiện trạng', index: 'StatusOfUseName', },
    { title: 'Tổng diện tích đất', index: 'TotalAreaValue', },
    { title: 'Tổng diện tích sàn sử dụng', index: 'TotalFloorValue', },
    { title: 'Giá đất/m2', render:'valueunitprice', index: 'UnitPriceValue', },
    { title: 'Thuế phi nông nghiệp', render:'valuetotal', index: 'TotalTaxNN', },
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
          icon: 'import',
          click: record => this.HouseFile(record),
          tooltip: 'Đính kèm file',
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá nhà này?',
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
    private md167HouseRepository: Md167HouseRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) {
    this.paging.TypeHouse = 1;
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
    this.paging.order_by = 'Id Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")`+ `or Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.md167HouseRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        this.paging.item_count = resp.metadata;
        this.data.forEach((item: any) => {
          const itemId = item["TypeHouse"];
          if (itemId == 1) {
            item["TypeHouseName"] = "Nhà phố";
          } else if (itemId === 2) {
            item["TypeHouseName"] = "Chung cư";
          }
        });
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
    const drawerRef = this.drawerService.create<AddorupdateHouseComponent>({
      nzTitle: record ? `Sửa nhà: "${record.HouseNumber}"` : 'Thêm mới nhà',
      // record.khoa_chinh
      nzWidth: '100vw',
      nzContent: AddorupdateHouseComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa nhà "${data.HouseNumber}" thành công!` : `Thêm mới nhà "${data.HouseNumber}" thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.md167HouseRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa nhà "${data.HouseNumber}" thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }
  onBack() {
    window.history.back();
  }

  importhouse() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel nhà`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.Md167House
        }
    });
  }
  importkios() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel kios`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.Md167Kios
        }
    });
  }

  HouseFile(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<Md167HouseFileComponent>({
        nzTitle: 'Danh sách file đính kèm',
        // record.khoa_chinh
        nzWidth: '75vw',
        nzContent: Md167HouseFileComponent,
        nzPlacement: 'left',
        nzContentParams: {
            record
        }
    });
    drawerRef.afterClose.subscribe((data: any) => {
        if (data) {
            let msg = data.Id ? `Thêm file thành công!` : ` Thêm file thành công!`;
            this.message.success(msg);
            this.getData();
        }
    });
}
}

