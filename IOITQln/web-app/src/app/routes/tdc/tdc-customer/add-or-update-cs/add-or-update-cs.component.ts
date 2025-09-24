import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TdcCustomerRepository } from 'src/app/infrastructure/repositories/tdcCustomer.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { map } from 'rxjs/operators';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TdcCustomerFileComponent } from '../tdc-customer-file/tdc-customer-file.component';

import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TdcMemberComponent } from '../tdc-member/tdc-member.component';
import { TdcAuthCustomerComponent } from '../tdc-auth-customer/tdc-auth-customer.component';

@Component({
  selector: 'app-add-or-update-cs',
  templateUrl: './add-or-update-cs.component.html'
})
export class AddOrUpdateCsComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @ViewChild('TdcCustomerFileComponent') private TdcCustomerFileComponent!: TdcCustomerFileComponent;
  @ViewChild('TdcMemberComponent') private TdcMemberComponent!: TdcMemberComponent;
  @ViewChild('TdcAuthCustomerComponent') private TdcAuthCustomerComponent!: TdcAuthCustomerComponent;

  public add = true;

  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;

  data_tableItemRef: any;
  invalid_tableItemRef = true;

  pdwTT_data = [];
  pdwLH_data = [];

  lane_dataTT: any[] = [];
  lane_dataLH: any[] = [];

  lane_data: any[] = [];

  tdcProject_data: any[] = [];
  tdcLand_data = [];
  tdcBlockHouse_data = [];
  tdcFloor_data = [];
  tdcApartment_data = [];
  tdcProject_Filter_data: any[] = [];
  data_landTDC_filter: NzSafeAny = [];
  data_BlockHouseTDC_filter: NzSafeAny = [];
  data_FloorTDC_filter: NzSafeAny = [];
  data_ApartmentTDC_filter: NzSafeAny = [];

  check = false;
  selectCorner: any;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private tdcCustomerRepository: TdcCustomerRepository,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private provinceRepository: ProvinceRepository,
    private laneRepository: LaneRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository
  ) {
    this.getDataProjectTDC();
  }

  ngOnInit(): void {
    this.getAllLaneData();
    if (localStorage.getItem('add') == 'false') this.add = false;
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined],
      FullName: [this.record ? this.record.FullName : undefined, [Validators.required]],
      CCCD: [this.record ? this.record.CCCD : undefined, Validators.pattern(/^\d{12}$/)],
      Phone: [this.record ? this.record.Phone : undefined, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)],
      Dob: [this.record ? convertDate(this.record.Dob) : undefined, [Validators.required]],
      Email: [this.record ? this.record.Email : undefined],
      Note: [this.record ? this.record.Note : undefined],
      //Tạm Trú
      AddressTT: [this.record ? this.record.AddressTT : undefined],
      LaneTT: [this.record ? this.record.LaneTT : undefined],
      WardTT: [this.record ? this.record.WardTT : undefined],
      DistrictTT: [this.record ? this.record.DistrictTT : undefined],
      ProvinceTT: [this.record ? this.record.ProvinceTT : undefined],
      PdwTT: [this.record ? [this.record.ProvinceTT, this.record.DistrictTT, this.record.WardTT] : undefined],
      //Liên Hệ
      AddressLH: [this.record ? this.record.AddressLH : undefined, [Validators.required]],
      LaneLH: [this.record ? this.record.LaneLH : undefined, [Validators.required]],
      WardLH: [this.record ? this.record.WardLH : undefined, [Validators.required]],
      DistrictLH: [this.record ? this.record.DistrictLH : undefined, [Validators.required]],
      ProvinceLH: [this.record ? this.record.ProvinceLH : undefined, [Validators.required]],
      PdwLH: [this.record ? [this.record.ProvinceLH, this.record.DistrictLH, this.record.WardLH] : undefined, [Validators.required]],

      NameFile: [this.record ? this.record.NameFile : undefined],
      NoteFile: [this.record ? this.record.NoteFile : undefined],
      tdcCustomerFiles: [this.record ? this.record.tdcCustomerFiles : [], []],
      tdcAuthCustomerDetailDatas: [this.record ? this.record.tdcAuthCustomerDetailDatas : [], []],

      TdcProjectId: [this.record ? this.record.TdcProjectId : undefined],
      LandId: [this.record ? this.record.LandId : undefined],
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined],
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined],
      TdcApartmentId: [this.record ? this.record.TdcApartmentId : undefined],
      Corner: [this.record ? this.record.Corner : false],
      Floor1: [this.record ? this.record.Floor1 : undefined],
      tdcMenberCustomerDatas: [this.record ? this.record.tdcMenberCustomerDatas : [], []]
    });

    this.getCascaderDataTT();
    this.getCascaderDataLH();

    if (this.record) {
      this.getLaneDataTT(this.record.WardTT, true);
    }
    if (this.record) {
      this.getLaneDataLH(this.record.WardLH, true);
    }
    if (this.record) {
      this.check = this.validateForm.value.Corner;
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataBlockHouseTDC(this.record.LandId);
      this.getDataFloorTDC(this.record.BlockHouseId);
      this.getApartmentTDC(this.record.FloorTdcId);
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.tdcCustomerFiles = this.TdcCustomerFileComponent.getValue();
    data.tdcMenberCustomerDatas = this.TdcMemberComponent.getValue();
    data.tdcAuthCustomerDetailDatas = this.TdcAuthCustomerComponent.getValue();


    const resp = data.Id ? await this.tdcCustomerRepository.update(data) : await this.tdcCustomerRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }
  // Bảng Cha
  async getCascaderDataTT() {
    try {
      this.loading = true;
      const resp = await this.provinceRepository.getCascaderData(1);

      if (resp.meta?.error_code == 200) {
        this.pdwTT_data = resp.data;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  async getCascaderDataLH() {
    try {
      this.loading = true;
      const resp = await this.provinceRepository.getCascaderData(1);

      if (resp.meta?.error_code == 200) {
        this.pdwLH_data = resp.data;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  async getLaneDataTT(wardId?: number, init: boolean = true) {
    if (!init) this.validateForm.get('LaneTT')?.setValue(undefined);
    this.lane_dataTT = [];
    if (!wardId) return;

    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Ward=${wardId}`;

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_dataTT = resp.data;

      if (resp.metadata == 1 && !init) {
        this.validateForm.get('LaneTT')?.setValue(this.lane_dataTT[0].Id);
      }
    }
  }

  async getLaneDataLH(wardId?: number, init: boolean = true) {
    if (!init) this.validateForm.get('LaneLH')?.setValue(undefined);
    this.lane_dataLH = [];
    if (!wardId) return;

    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Ward=${wardId}`;

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_dataLH = resp.data;

      if (resp.metadata == 1 && !init) {
        this.validateForm.get('LaneLH')?.setValue(this.lane_dataLH[0].Id);
      }
    }
  }

  changePdwTT() {
    let pdwTT = this.validateForm.value.PdwTT;

    if (pdwTT.length == 0) {
      this.validateForm.value.ProvinceTT = undefined;
      this.validateForm.value.DistrictTT = undefined;
      this.validateForm.value.WardTT = undefined;
      this.getLaneDataTT(undefined, false);
    } else {
      this.validateForm.get('ProvinceTT')?.setValue(pdwTT[0]);
      this.validateForm.get('DistrictTT')?.setValue(pdwTT[1]);
      this.validateForm.get('WardTT')?.setValue(pdwTT[2]);
      this.getLaneDataTT(pdwTT[2], false);
    }
    this.cdr.detectChanges();
  }

  changePdwLH() {
    let pdwLH = this.validateForm.value.PdwLH;

    if (pdwLH.length == 0) {
      this.validateForm.value.ProvinceLH = undefined;
      this.validateForm.value.DistrictLH = undefined;
      this.validateForm.value.WardLH = undefined;
      this.getLaneDataLH(undefined, false);
    } else {
      this.validateForm.get('ProvinceLH')?.setValue(pdwLH[0]);
      this.validateForm.get('DistrictLH')?.setValue(pdwLH[1]);
      this.validateForm.get('WardLH')?.setValue(pdwLH[2]);
      this.getLaneDataLH(pdwLH[2], false);
    }
    this.cdr.detectChanges();
  }

  async getAllLaneData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `1=1`;
    paging.select = 'Id,Name,Ward';

    const resp = await this.laneRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.lane_data = resp.data;
    }
  }

  async getDataProjectTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_data = resp.data;
    }
  }

  ////Lấy Dữ Liệu Lô
  async getDataLandTDC(tdcProjectId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TdcProjectId=${tdcProjectId}`;
    paging.select = 'Id,Code,Name,TdcProjectId';

    const resp = await this.landRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcLand_data = resp.data;
      this.data_landTDC_filter = this.tdcLand_data;
    }
  }

  async TDCLandByProject(event: number) {
    if (event) {
      this.getDataLandTDC(event);
    }
  }

  ///Lấy Dữ Liệu Khối
  async getDataBlockHouseTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId';

    const resp = await this.blockHouseRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcBlockHouse_data = resp.data;
      this.data_BlockHouseTDC_filter = this.tdcBlockHouse_data;
    }
  }
  TDCBlockHouseByLand(event: number) {
    if (event) {
      this.getDataBlockHouseTDC(event);
    }
  }

  /// Lấy Dữ Liệu Tầng
  async getDataFloorTDC(BlockHouseId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `BlockHouseId=${BlockHouseId}`;
    paging.select = 'Id,Code,Name,BlockHouseId,FloorNumber';

    const resp = await this.floorTdcRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcFloor_data = resp.data;
      this.data_FloorTDC_filter = this.tdcFloor_data;
    }
  }
  TDCFloorByBlockHouse(event: number) {
    if (event) {
      this.getDataFloorTDC(event);
    }
  }
  ///Lấy Dữ Liệu Căn
  async getApartmentTDC(FloorTdcId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `FloorTdcId=${FloorTdcId}`;
    paging.select = 'Id,Code,Name,FloorTdcId,Corner';

    const resp = await this.apartmentTdcRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcApartment_data = resp.data;
      this.data_ApartmentTDC_filter = this.tdcApartment_data;
    }
  }
  TDCApartmentrByFloor(event: any) {
    if (event) {
      this.getApartmentTDC(event);
      const selectedOption = this.data_FloorTDC_filter.find((option: any) => option.Id === event);
      this.validateForm.get('Floor1')?.setValue(this.calcFloor1(selectedOption));
    }
  }
  calcFloor1(data: any) {
    if (data.FloorNumber > 1) {
      return data.FloorNumber - 1;
    } else {
      return 'G';
    }
  }
  TdcCornerByApartment(event: any) {
    if (event) {
      this.selectCorner = this.data_ApartmentTDC_filter.find((i: any) => i.Id === event);
      if (this.selectCorner.Corner == true) this.check = true;
      else this.check = false;
    }
  }
}
