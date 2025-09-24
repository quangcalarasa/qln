import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Md167DelegateRepository } from 'src/app/infrastructure/repositories/md167delegate.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { ReportMd167Repository } from 'src/app/infrastructure/repositories/md167Report0repository';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { Md167TranferUnitRepository } from 'src/app/infrastructure/repositories/md167-tranfer-unit.repository';
import { Md167StateOfUseRepository } from 'src/app/infrastructure/repositories/md167-state-of-use.repository';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-report08',
  templateUrl: './report08.component.html',
  styles: [
  ]
})
export class Report08Component implements OnInit {
  validateForm!: FormGroup;

  tableData: any;
  lstDelegate: any;
  lstDistrict: any;
  lstWard: any;
  tranfer_unit_data: any;
  lstLane: any;
  nzFormat = 'dd/ MM/ yyyy';
  stateOfUse: any[] = [];
  house_data: any[] = [];

  loading = false;
  month:number;
  year:number;

  constructor(
    private fb: FormBuilder,
    private md167DelegateRepository: Md167DelegateRepository,
    private wardRepository: WardRepository,
    private districtRepository: DistrictRepository,
    private md167TranferUnitRepository: Md167TranferUnitRepository,
    private laneRepository: LaneRepository,
    private reportMd167Repository: ReportMd167Repository,
    private modalSrv: NzModalService,
    private md167ContractRepository: Md167ContractRepository,
    private md167StateOfUseRepository: Md167StateOfUseRepository,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(params => {
      this.month = params['month'];
      this.year = params['year'];
    });
  }

  ngOnInit(): void {
    this.getData()
    this.getHouseData();
    this.getStateOfUse();

    this.validateForm = this.fb.group({
      FromDate: undefined,
      HouseCode: undefined,
      ToDate: undefined,
      CustomertID: undefined,
      Md167TransferUnitId: undefined,
      DistrictId: undefined,
      WardId: undefined,
      LaneId: undefined,
      StatusOfUse: undefined,
      HouseId: undefined
    });

    if(this.month && this.year) {
      let d = new Date(this.year, this.month, 0);
      let date = d.getDate();
      let fromDate = `${this.year}-${this.month < 10 ? '0' + this.month : this.month}-01`;
      let toDate = `${this.year}-${this.month < 10 ? '0' + this.month : this.month}-${date}`;

      this.validateForm.get('FromDate')?.setValue(fromDate);
      this.validateForm.get('ToDate')?.setValue(toDate);
    }
    else if(this.year) {
      let fromDate = `${this.year}-01-01`;
      let toDate = `${this.year}-12-31`;

      this.validateForm.get('FromDate')?.setValue(fromDate);
      this.validateForm.get('ToDate')?.setValue(toDate);
    }

    if(this.validateForm.value.FromDate && this.validateForm.value.ToDate) {
      this.view();
    }
  }
  async view() {
    let data = { ...this.validateForm.getRawValue() };
    if (data.TransferUnit == "")
      data.TransferUnit = undefined;
    if (data.HouseCode == "")
      data.HouseCode = undefined;

    this.tableData = [];
    this.loading = true;
    const resp = await this.reportMd167Repository.getReport08(data);
    if (resp.meta?.error_code == 200) {
      this.tableData = resp.data;
      this.loading = false;
    }
  }
  async export() {
    await this.reportMd167Repository.ExportExcel08(this.tableData);
  }
  async changeDistrict() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `DistrictId=${this.validateForm.value.DistrictId}`;;
    paging.page_size = 0;
    paging.select = 'Id,Name';
    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.lstWard = respWard.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
    let pagingLane: GetByPageModel = new GetByPageModel();
    pagingLane.query = `District=${this.validateForm.value.WardId}`;
    pagingLane.page_size = 0;
    pagingLane.select = 'Id,Name';
    const respLane = await this.laneRepository.getByPage(pagingLane);
    if (respLane.meta?.error_code == 200) {
      this.lstLane = respLane.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }
  async changeWard() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `Ward=${this.validateForm.value.WardId}`;
    paging.page_size = 0;
    paging.select = 'Id,Name';
    const resp = await this.laneRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.lstLane = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
    console.log(this.lstLane);

  }
  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    paging.page = 1;
    paging.page_size = 1000;
    const resp4 = await this.md167TranferUnitRepository.getByPage(paging);

    if (resp4.meta?.error_code == 200) {
      this.tranfer_unit_data = resp4.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
    const respDelegate = await this.md167DelegateRepository.getByPage(paging);
    if (respDelegate.meta?.error_code == 200) {
      this.lstDelegate = respDelegate.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
    paging.query = `ProvinceId=2`;
    const respDistrict = await this.districtRepository.getByPage(paging);
    if (respDistrict.meta?.error_code == 200) {
      this.lstDistrict = respDistrict.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }

    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.lstWard = this.lstWard = respWard.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }

    paging.query = `Province=2`;
    const respLane = await this.laneRepository.getByPage(paging);
    if (respLane.meta?.error_code == 200) {
      this.lstLane = this.lstLane = respLane.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }

  }

  onBack() {
    window.history.back();
  }

  async getHouseData() {
    const resp = await this.md167ContractRepository.GetHouseData();

    if (resp.meta?.error_code == 200) {
      this.house_data = resp.data;
    }
  }

  async getStateOfUse() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.md167StateOfUseRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.stateOfUse = resp.data;
    }
  }
}
