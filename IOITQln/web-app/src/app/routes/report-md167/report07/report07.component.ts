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
import { formatDate } from '@angular/common';

@Component({
  selector: 'app-report07',
  templateUrl: './report07.component.html',
  styles: [
  ]
})
export class Report07Component implements OnInit {
  validateForm!: FormGroup;
  tableData: any;
  lstDelegate: any;
  lstDistrict: any;
  lstWard: any;
  lstLane: any;
  nzFormat = 'dd/ MM/ yyyy';
  house_data: any[] = [];

  loading = false;

  totalRow: any = undefined;

  constructor(
    private fb: FormBuilder,
    private md167DelegateRepository: Md167DelegateRepository,
    private wardRepository: WardRepository,
    private districtRepository: DistrictRepository,
    private laneRepository: LaneRepository,
    private reportMd167Repository: ReportMd167Repository,
    private modalSrv: NzModalService,
    private md167ContractRepository: Md167ContractRepository
  ) { }

  ngOnInit(): void {
    this.getData();
    this.getHouseData();
    this.validateForm = this.fb.group({
      FromDate: undefined,
      HouseCode: undefined,
      ToDate: undefined,
      CustomertID: undefined,
      DistrictId: undefined,
      WardId: undefined,
      LaneId: undefined,
      StatusOfUse: undefined,
      HouseId: undefined
    });
  }
  async view() {
    let data = { ...this.validateForm.getRawValue() };
    if (data.HouseCode == "")
      data.HouseCode = undefined;

    this.tableData = [];
    this.loading = true;
    this.totalRow = undefined;
    const resp = await this.reportMd167Repository.getReport07(data);
    if (resp.meta?.error_code == 200) {
      this.tableData = resp.data;
      this.totalRow = {
        Title: "TỔNG",
        RentCostContract: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.RentCostContract ?? 0) }, 0),
        Pay: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.Pay ?? 0) }, 0),
        Paid: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.Paid ?? 0) }, 0),
        PriceDiff: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.PriceDiff ?? 0) }, 0)
      };
      this.loading = false;
    }
  }
  async export() {
    await this.reportMd167Repository.ExportExcel07(this.tableData);

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
  }
  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    paging.page = 1;
    paging.page_size = 1000;
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
}
