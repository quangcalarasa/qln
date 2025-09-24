import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateRentingPriceComponent } from './add-or-update/add-or-update-renting-price.component';
import { RentingPriceRepository } from 'src/app/infrastructure/repositories/renting-price.repository';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { AccessKey, ImportHistoryTypeEnum, TypeAttributeCode } from 'src/app/shared/utils/enums';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { TypeDecree } from 'src/app/shared/utils/enums';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { TypeQD, LevelBlock } from 'src/app/shared/utils/consts';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-renting-price',
  templateUrl: './renting-price.component.html'
})
export class RentingPriceComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  dataAll: any[] = [];
  loading = false;
  levelblockmap_data = LevelBlock;
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENTING_PRICE_MANAGEMENT);
  decree_typies: any[] = [];
  unit_price_data: any[] = [];

  typehouse_data: any[] = [];

  typeDecree: number = TypeDecree.THONGTU;

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Quyết định', index: 'TypeQD', className: 'text-center', type: 'enum', enum: TypeQD },
    {
      title: 'Ngày hiệu lực',
      index: 'EffectiveDate',
      type: 'date',
      width: 120,
      sort: true,
      className: 'text-center',
      dateFormat: 'dd/MM/yyyy'
    },
    { title: 'Loại nhà', index: 'TypeBlockId', render: 'typeblock-column' },
    { title: 'Cấp nhà', index: 'LevelId', type: 'enum', enum: LevelBlock },
    { title: 'Giá thuê', render: 'priceClmn', className: 'text-right', width: 150 },
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
            title: 'Bạn có chắc chắn muốn xoá cấu hình giá thuê này?',
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
    private rentingPriceRepository: RentingPriceRepository,
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private typeAttributeRepository: TypeAttributeRepository,
    private decreeRepository: DecreeRepository,
    private unitPriceRepository: UnitPriceRepository,
    private TypeBlockRepository: TypeBlockRepository,
    private commonService: CommonService,
  ) { }

  ngOnInit(): void {
    this.getData();
    this.getTypeHouse();
    this.getDecree();
    this.getUnitPrice();
    console.log(this.levelblockmap_data);
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
      this.paging.query += ` and TypeBlockId=${this.query.type}`;
    }

    try {
      this.loading = true;
      const resp = await this.rentingPriceRepository.getByPage(this.paging);

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
    let typehouse_data = [...this.typehouse_data];
    let decree_typies = [...this.decree_typies];
    let unit_price_data = [...this.unit_price_data];

    const drawerRef = this.drawerService.create<AddOrUpdateRentingPriceComponent>({
      nzTitle: record ? `Sửa cấu hình giá thuê` : 'Thêm mới cấu hình giá thuê',
      nzWidth: '55vw',
      nzContent: AddOrUpdateRentingPriceComponent,
      nzContentParams: {
        record,
        typehouse_data,
        decree_typies,
        unit_price_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa cấu hình giá thuê thành công!` : `Thêm mới cấu hình giá thuê ${data.Name} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.rentingPriceRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa cấu hình giá thuê thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  async getTypeHouse() {
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.TypeBlockRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu loại căn.'
      });
    }
  }

  findApartmentType(id: number) {
    let item = this.typehouse_data.find(x => x.Id == id);
    return item ? item.Name : undefined;
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

  async getUnitPrice() {
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.unitPriceRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.unit_price_data = resp.data;
    }
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find(x => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }

  async csvExport() {
    const resp = await this.rentingPriceRepository.ExportExcel();
  }

  import() {
    this.drawerService.create<SharedImportExcelComponent>({
        nzTitle: `Import excel Hệ số lương`,
        nzWidth: '85vw',
        nzPlacement: 'left',
        nzContent: SharedImportExcelComponent,
        nzContentParams: {
            importHistoryType: ImportHistoryTypeEnum.NocRentingPrice
        }
    });
  }
}
