import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { TypeHouse } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import QueryModel from 'src/app/core/models/query-model';
import { FormBuilder, FormGroup } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';

@Component({
  selector: 'app-report-noc1',
  templateUrl: './report-noc1.component.html',
  styles: []
})
export class ReportNOC1Component implements OnInit {
  data: any;
  typehouse_data: any;
  validateForm!: FormGroup;
  lstDistrict: any;
  lstWard: any;
  lstLane: any;

  TypeHouse: { [key: number]: string } = TypeHouse;
  query: QueryModel = new QueryModel();
  constructor(
    private reportRepository: ReportRepository,
    private typeBlockRepository: TypeBlockRepository,
    private fb: FormBuilder,
    private wardRepository: WardRepository,
    private laneRepository: LaneRepository,
    private districtRepository: DistrictRepository
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      DistrictId: [undefined],
      WardId: [undefined],
      LaneId: [undefined],
      TypeBlock: [undefined]
    });
    this.getData();
    this.getTypeBlockData();
  }

  click() {
    this.getDataReport1();
  }

  async getDataReport1() {
    const resp = await this.reportRepository.ReportNOC1(this.validateForm.value);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }

  genTypeHouseName(env: any): string {
    return this.TypeHouse[env];
  }

  async getTypeBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.typeBlockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    }
  }

  async dowloadReportNOC1() {
    const resp = await this.reportRepository.ExportReportNOC1(this.data);
  }

  async changeDistrict() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `DistrictId=${this.validateForm.value.DistrictId}`;
    paging.page_size = 0;
    paging.select = 'Id,Name';
    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.lstWard = respWard.data;
    }
    let pagingLane: GetByPageModel = new GetByPageModel();
    pagingLane.query = `District=${this.validateForm.value.WardId}`;
    pagingLane.page_size = 0;
    pagingLane.select = 'Id,Name';
    const respLane = await this.laneRepository.getByPage(pagingLane);
    if (respLane.meta?.error_code == 200) {
      this.lstLane = respLane.data;
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
    }
  }

  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';

    paging.query = `ProvinceId=2`;
    const respDistrict = await this.districtRepository.getByPage(paging);
    if (respDistrict.meta?.error_code == 200) {
      this.lstDistrict = respDistrict.data;
    }

    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.lstWard = this.lstWard = respWard.data;
    }

    paging.query = `Province=2`;
    const respLane = await this.laneRepository.getByPage(paging);
    if (respLane.meta?.error_code == 200) {
      this.lstLane = this.lstLane = respLane.data;
    }
  }
}
