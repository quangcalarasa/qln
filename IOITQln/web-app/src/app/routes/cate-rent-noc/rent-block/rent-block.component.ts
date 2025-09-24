import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateRentBlockComponent } from './add-or-update/add-or-update-rent-block.component';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { TypeReportApply, LevelBlock, Decree } from 'src/app/shared/utils/consts';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { AccessKey, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
// import { SearchRentBlockComponent } from './search-rent-block/search-rent-block.component';
import { TypeBlockEntityEnum, TypeEditHistoryEnum } from 'src/app/shared/utils/enums';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { SharedConfirmUpdateMdComponent } from 'src/app/shared/components/confirm-update-md/confirm-update-md.component';
import { SharedConfirmUpdateListComponent } from 'src/app/shared/components/confirm-update-list/confirm-update-list.component';
import { CommonService } from 'src/app/core/services/common.service';
import { NocRentBlockFileComponent } from './noc-rent-block-file/noc-rent-block-file.component';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';

@Component({
  selector: 'app-cate-rent-block',
  templateUrl: './rent-block.component.html'
})
export class RentBlockComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENT_BLOCK);
  data: any[] = [];
  loading = false;

  TypeReportApply = TypeReportApply;
  TypeReportApplyEnum = TypeReportApplyEnum;
  typehouse_data: any[] = [];

  Decree = Decree;

  filter = false;

  districts: any;
  wards: any;
  lanes: any;

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Loại biên bản áp dụng', index: 'TypeReportApply', type: 'enum', enum: TypeReportApply },
    { title: 'Loại nhà', index: 'TypeBlockId', render: 'typeblock-column' },
    { title: 'Cấp nhà', index: 'levelBlockMaps', render: 'level-column' },
    { title: 'Mã định danh', render: 'codeClmn' },
    { title: 'Căn nhà số', index: 'Address' },
    // { title: 'Thông tin căn hộ', index: 'ApartmentInfo' },
    { title: 'Địa chỉ căn nhà', index: 'FullAddress', width: 200 },
    { title: 'Diện tích khuân viên của căn nhà', render: 'caClmn', className: 'text-center' },
    { title: 'Tổng diện tích sàn xây dựng', render: 'cavClmn', className: 'text-center' },
    { title: 'Tổng diện tích sử dụng', render: 'uavClmn', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 130,
      className: 'text-center',
      buttons: [
        {
          icon: 'history',
          iif: i => !i.edit,
          click: record => this.viewUpdateHistory(record),
          tooltip: 'Lịch sử cập nhật'
        },
        {
          icon: 'eye',
          iif: i => !i.edit,
          click: record => this.addOrUpdate(record, undefined, true),
          tooltip: 'Xem'
        },
        {
          icon: 'edit',
          click: record => this.updateConfirm(record),
          tooltip: 'Sửa',
          iif: i => !i.edit && this.role.Update,
        },
        {
          icon: 'import',
          click: record => this.BlockFile(record),
          tooltip: 'Đính kèm file',
        },
        {
          icon: 'delete',
          iif: i => this.role.Delete,
          type: 'del',
          tooltip: 'Xóa',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá căn nhà này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];

  extradata = 0;

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private blockRepository: BlockRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private typeBlockRepository: TypeBlockRepository,
    private commonService: CommonService,
    private wardRepository: WardRepository,
    private laneRepository: LaneRepository,
    private districtRepository: DistrictRepository
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getTypeBlockData();
    this.getDataDistrict();
    this.getDataWard();
    this.getDataLane();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        // this.addOrUpdate(e.dblClick?.item);
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
    this.paging.query = `TypeBlockEntity=${TypeBlockEntityEnum.BLOCK_RENT}`;
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Address.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TypeReportApply=${this.query.type}`
    }

    if (this.query.type2 != undefined) {
      this.paging.query += ` and District=${this.query.type2}`
    }

    if (this.query.type3 != undefined) {
      this.paging.query += ` and Ward=${this.query.type3}`
    }

    if (this.query.type4 != undefined) {
      this.paging.query += ` and Lane=${this.query.type4}`
    }

    try {
      this.loading = true;
      const resp = await this.blockRepository.getByPage(this.paging, this.query.type1);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        this.paging.item_count = resp.metadata;
        this.extradata = resp.extradata;
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

  addOrUpdate(record?: any, editHistory?: any, isViewRecord?: boolean): void {
    const drawerRef = this.drawerService.create<AddOrUpdateRentBlockComponent>({
      nzTitle: record ? (isViewRecord ? `Xem căn nhà: ${record.Address}` : `Sửa căn nhà: ${record.Address}`) : 'Thêm mới căn nhà',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateRentBlockComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        typehouse_data: this.typehouse_data,
        editHistory,
        isViewRecord
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa căn nhà ${data.Address} thành công!` : `Thêm mới căn nhà ${data.Address} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.blockRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa căn nhà ${data.Address} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  genLevelColumn(levelBlockMaps: any) {
    let res = '';

    levelBlockMaps.forEach((item: any) => {
      Object.keys(LevelBlock).some(v => {
        if (v === item.LevelId.toString()) {
          res =
            res == ''
              ? LevelBlock[v as unknown as keyof typeof LevelBlock]
              : res + ' + ' + LevelBlock[v as unknown as keyof typeof LevelBlock];
        }
      });
    });

    return res;
  }

  async getTypeBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.typeBlockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
      console.log(this.typehouse_data);
    }
  }

  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find(x => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }

  updateConfirm(record: any) {
    this.modalSrv.create({
      nzTitle: `Xác nhận sửa căn nhà ${record.Address}`,
      nzContent: SharedConfirmUpdateMdComponent,
      nzAutofocus: null,
      nzComponentParams: {
      },
      nzOnOk: async (res: any) => {
        let data = res.validateForm.value;
        this.addOrUpdate(record, data);
      }
    });
  }

  viewUpdateHistory(record: any) {
    this.modalSrv.create({
      nzTitle: `Lịch sử sửa căn nhà ${record.Address}`,
      nzContent: SharedConfirmUpdateListComponent,
      nzAutofocus: null,
      nzComponentParams: {
        TypeEditHistoryEnum: TypeEditHistoryEnum.BLOCK,
        targetId: record.Id
      },
      nzWidth: "1000px"
    });
  }

  BlockFile(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<NocRentBlockFileComponent>({
      nzTitle: 'Danh sách file đính kèm',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: NocRentBlockFileComponent,
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

  async getDataDistrict() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    paging.query = `ProvinceId=2`;

    const respDistrict = await this.districtRepository.getByPage(paging);
    if (respDistrict.meta?.error_code == 200) {
      this.districts = respDistrict.data;
    }
  }

  async getDataWard() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name,DistrictId';
    paging.query = `ProvinceId=2`;

    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.wards = respWard.data;
    }
  }

  async getDataLane() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name,Ward,District';
    paging.query = `Province=2`;

    const respLane = await this.laneRepository.getByPage(paging);
    if (respLane.meta?.error_code == 200) {
      this.lanes = respLane.data;
    }
  }

  changeDistrict() {
    this.query.type4 = undefined;
    this.query.type3 = undefined;
  }

  changeWard() {
    this.query.type4 = undefined;
  }
}
