import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateFloorTdcComponent } from './add-or-update-floortdc/add-or-update-floortdc.component';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { Router } from '@angular/router';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { read, utils, writeFile } from 'xlsx';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-floor-tdc',
  templateUrl: './floor-tdc.component.html',
  styles: [
  ]
})
export class FloorTdcComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  csv: any[] = [];

  data: any[] = [];
  loading = false;

  titleString?: string = 'Tầng';
  blockhouse_data:any[] = [];
  land_data: any[] = [];
  project_tdc_data: any[] = [];

  columns: STColumn[];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private floorTdcRepository: FloorTdcRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private router: Router,
    private blockHouseRepository: BlockHouseRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
  ) {
    const curentUrl = this.router.url;
    this.columns = [
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Mã tầng', index: 'Code' },
      { title: 'Tên tầng', index: 'Name' },
      { title: 'Khối', index: 'BlockHouseName'},
      { title: 'Số lượng căn', index:'ApartmentTdcCount' },
      { title: 'Ghi chú', index: 'Note' },
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
    ]
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getTDCProjectData();
    this.getLandData();
    this.getBlockHouseData();
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
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + `or Name.Contains("${this.query.txtSearch}")` + `or Note.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and BlockHouseId=${this.query.type}`
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TDCProjectId=${this.query.type}`
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and LandId=${this.query.type}`
    }

    try {
      this.loading = true;
      const resp = await this.floorTdcRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        this.paging.item_count = resp.metadata;
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu.'
        });
      }
      console.log(this.data)
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  addOrUpdate(record?: any): void {
    let add;
    let blockhouse_data = this.blockhouse_data;
    record ? add = true : add = false
    localStorage.setItem('add', add.toString())
    const drawerRef = this.drawerService.create<AddOrUpdateFloorTdcComponent>({
      nzTitle: record ? `Sửa ${this.titleString} : ${record.Code}` : `Thêm mới ${this.titleString}`,
      // record.khoa_chinh
      nzWidth: '60vw',
      nzContent: AddOrUpdateFloorTdcComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        blockhouse_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa ${this.titleString} ${data.Code} thành công!` : `Thêm mới ${this.titleString} ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.floorTdcRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa ${this.titleString} ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  async getTDCProjectData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = "Id,Name";

    const resp = await this.tdcProjectRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.project_tdc_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu.'
      });
    }
  }

  findTDCProject(id: number){
    let item = this.project_tdc_data.find(x => x.Id == id);
    return item ? item.Name : undefined;
  }

  async getLandData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = "Id,Name,TDCProjectId";

    const resp = await this.landRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.land_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu.'
      });
    }
  }

  findLand(id:number){
    let item = this.land_data.find(x => x.Id == id);
    return  item ? item.Name : undefined;
  }

  async getBlockHouseData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = "Id,Name";

    const resp = await this.blockHouseRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.blockhouse_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu.'
      });
    }
  }

  findBlockHouse(id:number){
    let item = this.blockhouse_data.find(x => x.Id == id);
    return  item ? item.Name : undefined;
  }

  import() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.TdcFloor
        }
    });
  }
  
  async csvExport() {
    const resp = await this.floorTdcRepository.ExportExcel();
  }
}
