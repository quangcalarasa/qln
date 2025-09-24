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
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-debt-info',
  templateUrl: './debt-info.component.html',
  styles: [
  ]
})
export class DebtInfoComponent implements OnInit {
  validateForm!: FormGroup;
  tableData: any;
  lstDelegate: any;
  lstDistrict: any;
  lstWard: any;
  lstLane: any;
  tranfer_unit_data: any;

  house_data: any[] = [];
  nzFormat = 'dd/ MM/ yyyy';
  loading = false;
  totalRow: any = undefined;
  month:number;
  year:number;

  constructor(
    private fb: FormBuilder,
    private md167DelegateRepository: Md167DelegateRepository,
    private md167TranferUnitRepository: Md167TranferUnitRepository,
    private wardRepository: WardRepository,
    private districtRepository: DistrictRepository,
    private laneRepository: LaneRepository,
    private reportMd167Repository: ReportMd167Repository,
    private modalSrv: NzModalService,
    private md167ContractRepository: Md167ContractRepository,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(params => {
      this.month = params['month'];
      this.year = params['year'];
    });
  }

  ngOnInit(): void {
    this.getData();
    this.getHouseData();

    this.validateForm = this.fb.group({
      FromDate: undefined,
      ToDate: undefined,
      CustomertID: undefined,
      DistrictId: undefined,
      WardId: undefined,
      LaneId: undefined,
      StatusOfUse: undefined,
      TransferUnit: undefined,
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
  change(t: number, x: number, y: number, value: any) {

    if (t == 1) {
      this.tableData[x].resReportDebtItems[y].AmountUsed = value;
    }
    else {
      this.tableData[x].resReportDebtItems[y].AmountSubmitted = value;
    }
    this.tableData[x].resReportDebtItems[y].AmountRemaining = this.tableData[x].resReportDebtItems[y].AmountToBePaid
      - this.tableData[x].resReportDebtItems[y].AmountUsed - this.tableData[x].resReportDebtItems[y].AmountSubmitted;
    this.tableData = [...this.tableData]
    this.calcTotalRow();
  }
  async view() {
    let data = { ...this.validateForm.getRawValue() };
    if (data.TransferUnit == "")
      data.TransferUnit = undefined;

    this.tableData = [];
    this.loading = true;
    this.totalRow = undefined;
    const resp = await this.reportMd167Repository.getReportDebtInfor(data);
    if (resp.meta?.error_code == 200) {
      this.tableData = resp.data;
      for (let i = 0; i < this.tableData.length; i++) {
        for (let j = 0; j < this.tableData[i].resReportDebtItems.length; j++) {
          this.tableData[i].resReportDebtItems[j].AmountRemaining = this.tableData[i].resReportDebtItems[j].AmountToBePaid;
        }
      }

      this.calcTotalRow();
      this.loading = false;
    }
  }
  async export() {
    await this.reportMd167Repository.ExportExcelDebtInfor(this.tableData);

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

  calcTotalRow() {
    this.totalRow = {
      Title: "TỔNG",
      AmountToBePaid: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountToBePaid) ?? 0) }, 0)) }, 0),
      AmountPaid: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountPaid) ?? 0) }, 0)) }, 0),
      AmountDiff: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountDiff) ?? 0) }, 0)) }, 0),
      AmountUsed: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountUsed) ?? 0) }, 0)) }, 0),
      AmountSubmitted: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountSubmitted) ?? 0) }, 0)) }, 0),
      AmountRemaining: this.tableData.reduce((x: number, curItem: any) => { return x + (curItem.resReportDebtItems.reduce((y: number, curChildItem: any) => { return y + (Number(curChildItem.AmountRemaining) ?? 0) }, 0)) }, 0)
    };
  }
}
