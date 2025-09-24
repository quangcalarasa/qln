import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { CommonService } from 'src/app/core/services/common.service';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { G2PieClickItem, G2PieComponent, G2PieData, G2PieModule } from '@delon/chart/pie';
import { G2BarData, G2BarModule } from '@delon/chart/bar';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Md167DashboardRepository } from 'src/app/infrastructure/repositories/md167-dashboard.repository';
import { AccessKey } from 'src/app/shared/utils/enums';
import { Router } from '@angular/router';

interface NewHouseBaseReq {
  type: string;
  month: number;
  year: number;
}

interface RevenueReq {
  month: number;
  year: number;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  // changeDetection: ChangeDetectionStrategy.OnPush

})

export class DashboardComponent implements OnInit {
  // @ViewChild('pie', { static: false}) readonly pie!: G2PieComponent;
  // salesPieData: G2PieData[] = [];
  // total = '';
  // profileForm!: FormGroup;

  // rowHeight = 60;
  // nzPageSize: number = 10;

  // value?: number;
  // str?: string;
  // items = [
  //   { Id: 1, Name: "1" },
  //   { Id: 2, Name: "2" },
  //   { Id: undefined, Name: "3" },
  //   { Id: 4, Name: "4" },
  //   { Id: 5, Name: "5" },
  //   { Id: undefined, Name: "6" },
  //   { Id: undefined, Name: "7" }

  // ];

  roleMd167Dashboard = this.commonService.CheckAccessKeyRole(AccessKey.MD167_DASHBOARD);

  houseBaseData: any;

  newHouseBaseReq: NewHouseBaseReq = { type: "1", month: new Date().getMonth() + 1, year: new Date().getFullYear() };
  newHouseBaseData: any[] = [];

  revenueBoxYearReq: RevenueReq = { month: new Date().getMonth() + 1, year: new Date().getFullYear() };
  revenueBoxYearData: any;

  revenueBoxMonthReq: RevenueReq = { month: new Date().getMonth() + 1, year: new Date().getFullYear() };
  revenueBoxMonthData: any;

  taxData: any[] = [];

  chartMinibarData = [{ x: '1', y: 7 }, { x: '2', y: 5 }, { x: '3', y: 4 }, { x: '4', y: 2 }, { x: '5', y: 4 }, { x: '6', y: 7 }, { x: '7', y: 5 }, { x: '8', y: 6 }, { x: '9', y: 1 }]

  constructor(private md167DashboardRepository: Md167DashboardRepository, private commonService: CommonService, private msg: NzMessageService, private router: Router) {
    // this.test();
  }

  ngOnInit(): void {
    // this.nzPageSize = Math.max(Math.floor((window.innerHeight - 260) / this.rowHeight), 3);
    // this.profileForm = this.fb.group({
    //   search_text: [''],
    //   rangePicker: [[]],
    // });

    if (this.roleMd167Dashboard.ViewOrActionSpecial) {
      this.GetHouseBase();
      this.GetNewHouseBase();
      this.GetRevenueBoxYearData();
      this.GetRevenueBoxMonthData();
      this.GetTaxInfo();
    }
  }

  async GetHouseBase() {
    const resp = await this.md167DashboardRepository.GetHouseBase();
    if (resp.meta?.error_code == 200) {
      this.houseBaseData = resp.data;
    }
  }

  async GetNewHouseBase() {
    const resp = await this.md167DashboardRepository.GetNewHouseBase(this.newHouseBaseReq.type == "1" ? this.newHouseBaseReq.month : 0, this.newHouseBaseReq.year);
    if (resp.meta?.error_code == 200) {
      this.newHouseBaseData = resp.data;
    }
  }

  async GetRevenueBoxYearData() {
    const resp = await this.md167DashboardRepository.GetRevenue(0, this.revenueBoxYearReq.year);
    if (resp.meta?.error_code == 200) {
      this.revenueBoxYearData = resp.data;
    }
  }

  async GetRevenueBoxMonthData() {
    const resp = await this.md167DashboardRepository.GetRevenue(this.revenueBoxMonthReq.month, this.revenueBoxMonthReq.year);
    if (resp.meta?.error_code == 200) {
      this.revenueBoxMonthData = resp.data;
    }
  }

  async GetTaxInfo() {
    const resp = await this.md167DashboardRepository.GetTaxInfo();
    if (resp.meta?.error_code == 200) {
      this.taxData = resp.data;
    }
  }

  //#region Md167 data  

  //#endregion

  // async onSubmit() { }

  // changeValue(value: number) {
  //   this.str = this.commonService.convertMoneyToString(value);
  // }

  // test(): void {
  //   const rv = (min: number = 0, max: number = 5000): number => Math.floor(Math.random() * (max - min + 1) + min);
  //   this.salesPieData = [
  //     {
  //       x: 'A',
  //       y: rv()
  //     },
  //     {
  //       x: 'B',
  //       y: rv()
  //     },
  //     {
  //       x: 'C',
  //       y: rv()
  //     },
  //     {
  //       x: 'D',
  //       y: rv()
  //     },
  //     {
  //       x: 'E',
  //       y: rv()
  //     }
  //   ];
  //   if (Math.random() > 0.5) {
  //     this.salesPieData.push({
  //       x: 'F',
  //       y: rv()
  //     });
  //   }
  //   this.total = `${this.salesPieData.reduce((pre, now) => now.y + pre, 0).toFixed(2)} Đồng`;
  //   if (this.pie) {
  //     setTimeout(() => this.pie.changeData());
  //   }
  //   this.salesData = this.genData();
  // }

  // salesData = this.genData();

  // private genData(): G2BarData[] {
  //   return new Array(12).fill({}).map((_i, idx) => ({
  //     x: `Tháng ${idx + 1}`,
  //     y: Math.floor(Math.random() * 1000) + 200,
  //     // color: idx > 5 ? '#f50' : undefined
  //   }));
  // }

  // format(val: number): string {
  //   return `&yen ${val.toFixed(2)}`;
  // }

  // handleClick(data: G2PieClickItem): void {
  //   this.msg.info(`${data.item.x} - ${data.item.y}`);
  // }

  goToReport(cs: number) {
    switch (cs) {
      case 1:
        this.router.navigate(['/cate-md167/report-md167/report08-md167'], { queryParams: { year: this.newHouseBaseReq.year, month: (this.newHouseBaseReq.type == "1" ? this.newHouseBaseReq.month : undefined) } });
        break;
      case 2:
        this.router.navigate(['/cate-md167/report-md167/report-payment']);
        break;
      case 3:
        this.router.navigate(['/cate-md167/report-md167/info-debt-md167'], { queryParams: { year: this.revenueBoxYearReq.year } });
        break;
      case 4:
        this.router.navigate(['/cate-md167/report-md167/info-debt-md167'], { queryParams: { year: this.revenueBoxMonthReq.year, month: this.revenueBoxMonthReq.month } });
        break;
      default:
        break;
    }
  }
}
