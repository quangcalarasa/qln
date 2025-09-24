import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { ReportMd167Repository } from 'src/app/infrastructure/repositories/md167Report0repository';
import { Md167ManPaymentRepository } from 'src/app/infrastructure/repositories/md167-manage-payment.repository';

@Component({
  selector: 'app-report-payment',
  templateUrl: './report-payment.component.html',
  styles: [
  ]
})
export class ReportPaymentComponent implements OnInit {
  validateForm!: FormGroup;
  tableData: any;
  lstDelegate: any;
  lstDistrict: any;
  lstWard: any;
  lstLane: any;
  tranfer_unit_data: any;
  years: number[] = Array.from({ length: 100 }, (_, index) => 2000 + index);
  md167_house_data: any[] = [];
  nzFormat = 'dd/ MM/ yyyy';
  loading = false;
  constructor(
    private fb:FormBuilder,
    private md167ManPaymentRepository: Md167ManPaymentRepository,
    private wardRepository: WardRepository,
    private districtRepository: DistrictRepository,
    private laneRepository: LaneRepository,
    private reportMd167Repository: ReportMd167Repository,
    private modalSrv: NzModalService,
  ) { }

  ngOnInit(): void {
    this.getData();
    this.getHouseData();
    this.validateForm = this.fb.group({
      FromYear:undefined,
      ToYear:undefined,
      DistrictId:undefined,
      WardId:undefined,
      LaneId:undefined,
      HouseId:undefined,
    })
  }

  //lấy theo quận
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

  //lấy theo phường
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

  onBack() {
    window.history.back();
  }

  async view() {
    let data = { ...this.validateForm.getRawValue() };
    if (data.HouseCode == "")
      data.HouseCode = undefined;

    this.tableData = [];
    this.loading = true;
    const resp = await this.reportMd167Repository.getReportPayment(data);
    if (resp.meta?.error_code == 200) {
      this.tableData = resp.data;
      this.loading = false;
    }
  }

  async getData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    paging.page = 1;
    paging.page_size = 1000;
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


  async getHouseData() {
    const resp = await this.md167ManPaymentRepository.getKiosAndHouse();
    if (resp.meta?.error_code == 200) {
      this.md167_house_data = resp.data;
    }
  }

}
