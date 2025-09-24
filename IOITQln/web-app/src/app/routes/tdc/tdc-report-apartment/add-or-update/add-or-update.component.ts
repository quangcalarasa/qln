import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

import { ReportApartmentRepository } from 'src/app/infrastructure/repositories/tdc-report-apartment.repository';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { DistrictAllocasionApartmentComponent } from '../district-allocasion-apartment/district-allocasion-apartment.component';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateApartmentReportComponent implements OnInit {
  @ViewChild('districtAllocasionApartmentComponent') districtAllocasionApartmentComponent!: DistrictAllocasionApartmentComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;
  myForm = new FormGroup({
    HandoverPublic: new FormControl(false),
    HandoverOther: new FormControl(false),
    HandoverCenter: new FormControl(false)
  })
  decision_data: any[]=[];
  @Input() record: NzSafeAny;
  @Input() typedecision_data: NzSafeAny;
  @Input() typelegal_data: NzSafeAny;
  tdcProject_data: any[] = [];
  tdcLand_data = [];
  tdcBlockHouse_data = [];
  tdcFloor_data = [];
  tdcApartment_data = [];
  tdcDistrict_data: any[] = [];
  tdcDistrict_Project_data: any[] = [];
  data_landTDC_filter: NzSafeAny = [];
  data_BlockHouseTDC_filter: NzSafeAny = [];
  data_FloorTDC_filter: NzSafeAny = [];
  data_ApartmentTDC_filter: NzSafeAny = [];
  data_ProjectTDC_filter: NzSafeAny = [];
  selectedRoomNumber: number;
  selectedContrustionBuild: number;

  selectedValue ='1';
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private reportApartmentRepository:ReportApartmentRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository,
    private districtRepository: DistrictRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined, [Validators.required]],
      // Identifier: [this.record ? this.record.Identifier : undefined, []],//id quyết định
      TypeDecisionId: [this.record ? this.record.TypeDecisionId : undefined, []],//id quyết định
      TypeLegalId: [this.record ? this.record.TypeLegalId : undefined, [Validators.required]],//id pháp lý tiếp nhận
      BlockHouseId: [this.record ? this.record.BlockHouseId : undefined, [Validators.required]],//id khối
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],//id lô
      FloorTdcId: [this.record ? this.record.FloorTdcId : undefined, [Validators.required]],//id tầng
      TdcProjectId: [this.record ? this.record.TdcProjectId : undefined, [Validators.required]],// id dự án
      ApartmentTdcId: [this.record ? this.record.ApartmentTdcId : undefined, [Validators.required]],// id căn
      Qantity: [this.record ? this.record.Qantity : undefined, []],//Số lượng phân bổ theo quy định
      ReceiveNumber: [this.record ? this.record.ReceiveNumber : undefined, []],//Số lượng tiếp nhận theo quy định
      HandOverYear: [this.record ? this.record.HandOverYear : undefined, []],//năm bàn giao
      OverYear: [this.record ? (this.record.OverYear ? this.record.OverYear.toString() : undefined) : "1", []],//chọn theo năm bàn giao
      NoteHandoverPublic:[this.record ? this.record.NoteHandoverPublic : undefined],//ghi chú theo DVCI bàn giao
      NoteHandoverCenter:[this.record ? this.record.NoteHandoverCenter : undefined],//ghi chú theo Trung tâm giao
      NoteHandoverOther:[this.record ? this.record.NoteHandoverOther : undefined],//ghi chú theo bàn giao khác
      DistrictProjectId: [this.record ? this.record.DistrictProjectId : undefined, [Validators.required]],//id quận/huyện của dự án
      ReceptionDate: [this.record ? convertDate(this.record.ReceptionDate) : undefined],// thời gián tiếp nhận
      ReceptionTime: [this.record ? (this.record.ReceptionTime ? this.record.ReceptionTime.toString() : undefined) : "1", []],// chọn theo tiếp nhận
      HandOver: [this.record ? (this.record.HandOver ? this.record.HandOver.toString() : undefined) : "1", []],//chọn theo bàn giao
      ReasonReceivedYet:[this.record ? this.record.ReasonReceivedYet : undefined],// lý do chưa tiếp nhận
      Reminded:[this.record ? this.record.Reminded : undefined],// đã nhắc nhở
      ReasonNotReceived:[this.record ? this.record.ReasonNotReceived : undefined],// lý do ko tiếp nhận
      HandoverNumber: [this.record ? this.record.HandoverNumber : undefined, []],
      HandoverPublic: [this.record ? this.record.HandoverPublic : undefined, []],//dvci bàn giao
      HandoverOther: [this.record ? this.record.HandoverOther : undefined, []],//bàn giao khác  
      HandoverCenter: [this.record ? this.record.HandoverCenter : undefined, []],//trung tâm bàn giao
      TdcApartmentCountRoom: [this.record ? this.record.TdcApartmentCountRoom : undefined],//số lượng phòng ngủ
      TdcApartmentArea: [this.record ? this.record.TdcApartmentArea : undefined],//diện tích căn h
      Note:[this.record ? this.record.Note : undefined],
      districtAllocasionApartment:[this.record ? this.record.districtAllocasionApartment: []],
    });
    
    this.getDistrictProjectTDC();
    if (this.record) {
      this.getDataProjectTDC(this.record.DistrictProjectId);
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataBlockHouseTDC(this.record.LandId);
      this.getDataFloorTDC(this.record.BlockHouseId);
      this.getApartmentTDC(this.record.FloorTdcId);
    }
  }
  
  changeData(data: any) {
    this.validateForm.get('districtAllocasionApartment')?.setValue(data);
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.districtAllocasionApartment = [...this.districtAllocasionApartmentComponent.getValue()];
    const isHandoverPublic = this.validateForm.get('HandoverPublic')?.value;
    data.isHandoverPublic = isHandoverPublic;
    const isHandoverOther = this.validateForm.get('HandoverOther')?.value;
    data.isHandoverOther = isHandoverOther;
    const isHandoverCenter = this.validateForm.get('HandoverCenter')?.value;
    data.isHandoverCenter = isHandoverCenter;
    const resp = data.Id 
      ? await this.reportApartmentRepository.update(data) 
      : await this.reportApartmentRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }

  async getDataProjectTDC(tdcDistrictId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `District=${tdcDistrictId}`
    paging.select = `Id, Code, Name, District`
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcProject_data = resp.data;
      this.data_ProjectTDC_filter = this.tdcProject_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  async ProjectByDistrict(event: number){
    if(event){
      this.getDataProjectTDC(event);
    }
  }

  async getDistrictProjectTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.districtRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcDistrict_Project_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  async getDataLandTDC(tdcProjectId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TdcProjectId=${tdcProjectId}`;
    paging.select = 'Id,Code,Name,TdcProjectId';

    const resp = await this.landRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcLand_data = resp.data;
      this.data_landTDC_filter = this.tdcLand_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  async TDCLandByProject(event: number) {
    if (event) {
      this.getDataLandTDC(event);
    }
  }

  async getDataBlockHouseTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId';

    const resp = await this.blockHouseRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.tdcBlockHouse_data = resp.data;
      this.data_BlockHouseTDC_filter = this.tdcBlockHouse_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  TDCBlockHouseByLand(event: number) {
    if (event) {
      this.getDataBlockHouseTDC(event);
    }
  }

  async getDataFloorTDC(BlockHouseId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `BlockHouseId=${BlockHouseId}`;
    paging.select = 'Id,Code,Name,BlockHouseId';

    const resp = await this.floorTdcRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.tdcFloor_data = resp.data;
      this.data_FloorTDC_filter = this.tdcFloor_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  TDCFloorByBlockHouse(event: number) {
    if (event) {
      this.getDataFloorTDC(event);
    }
  }

  async getApartmentTDC(FloorTdcId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `FloorTdcId=${FloorTdcId}`;
    paging.select = 'Id,Code,Name,FloorTdcId,RoomNumber,ContrustionBuild';

    const resp = await this.apartmentTdcRepository.getByPage(paging);
  
    if (resp.meta?.error_code == 200) {
      this.tdcApartment_data = resp.data;
      this.data_ApartmentTDC_filter = this.tdcApartment_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  TDCApartmentrByFloor(event: any) {
    if (event) {
      this.getApartmentTDC(event);
    }
  }

  TDCApartmentbyCount(event: any){
    if(event){
      this.getApartmentTDC(event);
      const selectedOption = this.data_ApartmentTDC_filter.find((option: any) => option.Id === event);
      this.validateForm.get('TdcApartmentCountRoom')?.setValue(this.calcCountRoom(selectedOption));
      this.validateForm.get('TdcApartmentArea')?.setValue(this.calcCountBuild(selectedOption));
    }
  }

  calcCountRoom(data: any) {
    if (data.RoomNumber > 1) {
      return data.RoomNumber;
    }
  }

  calcCountBuild(data: any) {
    if (data.ContrustionBuild > 1) {
      return data.ContrustionBuild;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

}
