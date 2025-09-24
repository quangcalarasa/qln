import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

import { ReportPlatformRepository } from 'src/app/infrastructure/repositories/tdc-report-platform.repository';
import { TdcReportPlatformComponent } from '../tdc-report-platform.component'; 
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { PlatformTdcRepository } from 'src/app/infrastructure/repositories/platform-tdc.repository'; 
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { DistrictAllocasionPlatformComponent } from '../district-allocasion-platform/district-allocasion-platform.component';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdatePlatformReportComponent implements OnInit {
  @ViewChild('districtAllocasionPlatformComponent') districtAllocasionPlatformComponent!: DistrictAllocasionPlatformComponent;
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

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
  tdcPlatform_data = [];
  tdcDistrict_data: any[] = [];
  tdcDistrict_Project_data: any[] = [];
  data_landTDC_filter: NzSafeAny = [];
  data_PlatformTDC_filter: NzSafeAny = [];
  data_ProjectTDC_filter: NzSafeAny = [];
  selectedPlatcount: number;
  selectedPlatArea: number;
  selectedPlatLength: number;
  selectedPlatWidth: number;
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private reportPlatformRepository:ReportPlatformRepository,
    private landRepository: LandRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private platformTdcRepository: PlatformTdcRepository,
    private districtRepository: DistrictRepository,
  ) {
    
   }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined, [Validators.required]],
      TypeDecisionId: [this.record ? this.record.TypeDecisionId : undefined, [Validators.required]],
      // Identifier: [this.record ? this.record.Identifier : undefined, []],//id quyết định
      TypeLegalId: [this.record ? this.record.TypeLegalId : undefined, [Validators.required]],
      LandId: [this.record ? this.record.LandId : undefined, [Validators.required]],
      TdcProjectId: [this.record ? this.record.TdcProjectId : undefined, [Validators.required]],
      PlatformTdcId: [this.record ? this.record.PlatformTdcId : undefined, [Validators.required]],
      Qantity: [this.record ? this.record.Qantity : undefined, []],
      ReceiveNumber: [this.record ? this.record.ReceiveNumber : undefined, []],
      HandOverYear: [this.record ? this.record.HandOverYear : undefined, []],
      NoteHandoverPublic:[this.record ? this.record.NoteHandoverPublic : undefined],//ghi chú theo DVCI bàn giao
      NoteHandoverCenter:[this.record ? this.record.NoteHandoverCenter : undefined],//ghi chú theo Trung tâm giao
      NoteHandoverOther:[this.record ? this.record.NoteHandoverOther : undefined],//ghi chú theo bàn giao khác
      DistrictProjectId: [this.record ? this.record.DistrictProjectId : undefined, [Validators.required]],
      ReceptionDate: [this.record ? convertDate(this.record.ReceptionDate) : undefined, [Validators.required]],// thời gián tiếp nhận
      ReceptionTime: [this.record ? (this.record.ReceptionTime ? this.record.ReceptionTime.toString() : undefined) : "1", []],
      HandOver: [this.record ? (this.record.HandOver ? this.record.HandOver.toString() : undefined) : "1", []],
      ReasonReceivedYet:[this.record ? this.record.ReasonReceivedYet : undefined],// lý do chưa tiếp nhận
      Reminded:[this.record ? this.record.Reminded : undefined],// đã nhắc nhở
      ReasonNotReceived:[this.record ? this.record.ReasonNotReceived : undefined],// lý do ko tiếp nhận
      HandoverNumber: [this.record ? this.record.HandoverNumber : undefined, []],
      HandoverPublic: [this.record ? this.record.HandoverPublic : undefined, []],
      HandoverOther: [this.record ? this.record.HandoverOther : undefined, []],
      HandoverCenter: [this.record ? this.record.HandoverCenter : undefined, []],
      TdcLength: [this.record ? this.record.TdcLength : undefined],
      TdcWidth: [this.record ? this.record.TdcWidth : undefined],
      TdcPlatformArea: [this.record ? this.record.TdcPlatformArea : undefined],
      PlatCount: [this.record ? this.record.PlatCount : undefined],
      Note:[this.record ? this.record.Note : undefined],
      districtAllocasionPlatform:[this.record ? this.record.districtAllocasionPlatform: []],
    });
    
    // this.getDataProjectTDC(this.record.DistrictProjectId)
    this.getDistrictProjectTDC();
    if (this.record) {
      this.getDataProjectTDC(this.record.DistrictProjectId);
      this.getDataLandTDC(this.record.TdcProjectId);
      this.getDataPlatformTDC(this.record.LandId);
    }
  }
  
  changeData(data: any) {
    this.validateForm.get('districtAllocasionPlatform')?.setValue(data);
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.districtAllocasionPlatform = [...this.districtAllocasionPlatformComponent.getValue()];
    const isHandoverPublic = this.validateForm.get('HandoverPublic')?.value;
    data.isHandoverPublic = isHandoverPublic;
    const isHandoverOther = this.validateForm.get('HandoverOther')?.value;
    data.isHandoverOther = isHandoverOther;
    const isHandoverCenter = this.validateForm.get('HandoverCenter')?.value;
    data.isHandoverCenter = isHandoverCenter;
    const resp = data.Id 
      ? await this.reportPlatformRepository.update(data) 
      : await this.reportPlatformRepository.addNew(data);
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

  async getDataPlatformTDC(LandId: number) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `LandId=${LandId}`;
    paging.select = 'Id,Code,Name,LandId,TDCProjectId,Platcount,LandArea,LengthArea,WidthArea';

    const resp = await this.platformTdcRepository.getByPage(paging);
    
    if (resp.meta?.error_code == 200) {
      this.tdcPlatform_data = resp.data;
      this.data_PlatformTDC_filter = this.tdcPlatform_data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  TDCPlatformByLand(event: any) {
    if (event) {
      this.getDataPlatformTDC(event);
    }
  }

  TDCPlatformbyCount(event: any){
    if(event){
      this.getDataPlatformTDC(event);
      const selectedOption = this.data_PlatformTDC_filter.find((option: any) => option.Id === event);
      this.validateForm.get('PlatCount')?.setValue(this.calcCountPlat(selectedOption));
      this.validateForm.get('TdcPlatformArea')?.setValue(this.calcLandArea(selectedOption));
      this.validateForm.get('TdcLength')?.setValue(this.calcTdcLength(selectedOption));
      this.validateForm.get('TdcWidth')?.setValue(this.calcTdcWidth(selectedOption));
    }
  }

  calcCountPlat(data: any) {
    if (data.Platcount >= 0) {
      return data.Platcount;
    }
  }

  calcLandArea(data: any) {
    if (data.LandArea >= 0) {
      return data.LandArea;
    }
  }

  calcTdcLength(data: any) {
    if (data.LengthArea >= 0) {
      return data.LengthArea;
    }
  }

  calcTdcWidth(data: any) {
    if (data.WidthArea >= 0) {
      return data.WidthArea;
    }
  }


  close(): void {
    this.drawerRef.close();
  }

}
