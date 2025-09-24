import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import { GetByPageLandPriceModel } from 'src/app/core/models/get-by-page-model';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateMd167LandPriceComponent } from './add-or-update/add-or-update.component';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { Md167LandPriceRepository } from 'src/app/infrastructure/repositories/md167landprice.repository';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { TypeDecree } from 'src/app/shared/utils/enums';
import { Decree } from 'src/app/shared/utils/consts';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';

@Component({
  selector: 'app-land-price',
  templateUrl: './land-price.component.html',
  styles: [
  ]
})
export class LandPriceComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageLandPriceModel = new GetByPageLandPriceModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  // decree_type1_data = [];
  decree_type1_data = Decree;
  decree_type2_data: any[] = [];

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Nghị định áp dụng cho biên bản', index: 'DecreeType1Id', type: 'enum', enum: Decree },
    { title: 'Văn bản pháp luật liên quan', index: 'DecreeType2Name' },
    { title: 'Bảng giá đất ở Quận/huyện - Tỉnh/tp', render: 'detail-column' },
    { title: 'Căn cứ', index: 'Des' },
    // { title: 'Đoạn đường đến', index: 'LaneEndName' },
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
            title: 'Bạn có chắc chắn muốn xoá thông tin giá đất này?',
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
    private md167LandPriceRepository: Md167LandPriceRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private decreeRepository: DecreeRepository
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    // this.getDataDecreeType1();
    this.getDataDecreeType2();
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
    this.paging.landPriceType = 2;

    if (this.query.type1 != undefined) {
      this.paging.query += ` and DecreeType1Id=${this.query.type1}`
  }

  if (this.query.type2 != undefined) {
      this.paging.query += ` and DecreeType2Id=${this.query.type2}`
  }

    // if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
    //   if (this.query.txtSearch.trim() != '')
    //     this.paging.query += ` and Des.Contains("${this.query.txtSearch}")`;
    // }

    try {
      this.loading = true;
      const resp = await this.md167LandPriceRepository.getByPage(this.paging, this.query.txtSearch);

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
    const drawerRef = this.drawerService.create<AddOrUpdateMd167LandPriceComponent>({
      nzTitle: record ? `Sửa thông tin giá đất` : 'Thêm mới thông tin giá đất',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateMd167LandPriceComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        // decree_type1_data: this.decree_type1_data,
        decree_type2_data: this.decree_type2_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa thông tin giá đất thành công!` : `Thêm mới thông tin giá đất thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  
  async delete(data: any) {
    const resp = await this.md167LandPriceRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa thông tin giá đất thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  // async getDataDecreeType1() {
  //     let paging: GetByPageModel = new GetByPageModel();
  //     paging.page_size = 0;
  //     paging.query = `TypeDecree=${TypeDecree.NGHIDINH}`;
  //     paging.select = "Id,Code";

  //     const resp = await this.decreeRepository.getByPage(paging, TypeDecree.NGHIDINH);

  //     if (resp.meta?.error_code == 200) {
  //         this.decree_type1_data = resp.data;
  //     }
  // }

  async getDataDecreeType2() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TypeDecree=${TypeDecree.THONGTU}`;
    paging.select = "Id,Code";

    const resp = await this.decreeRepository.getByPage(paging, TypeDecree.THONGTU);

    if (resp.meta?.error_code == 200) {
      this.decree_type2_data = resp.data;
    }
  }

  import() {
    const drawerRef = this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel bảng giá đất cho nhà ở 167`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.Md167Landprice
        }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      this.getData();
    });
  }
}

