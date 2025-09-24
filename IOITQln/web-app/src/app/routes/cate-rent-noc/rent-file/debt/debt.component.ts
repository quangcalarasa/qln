import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { PaymentComponent } from '../payment/payment.component';
import { ReceiptsComponent } from '../receipts/receipts.component';
import { PromissoryRepository } from 'src/app/infrastructure/repositories/Promissory.repository';
import { SettingsService } from '@delon/theme';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { DebtsTableRepository } from 'src/app/infrastructure/repositories/DebtsTable.repository';
import { DebtsRepository } from 'src/app/infrastructure/repositories/Debts.repository';
import { ImportDebtComponent } from '../import-debt/import-debt.component';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-debt',
  templateUrl: './debt.component.html',
  styles: []
})
export class DebtComponent implements OnInit {
  @Input() record: NzSafeAny;
  @ViewChild('PaymentComponent') PaymentComponent!: PaymentComponent;
  @ViewChild('ReceiptsComponent') ReceiptsComponent!: ReceiptsComponent;

  validateForm!: FormGroup;

  public add = true;
  loading: boolean = false;

  dataBCT: any;
  dataRentFile: any[] = [];
  data_GetWorkSheet22: any[] = [];
  showScreen = false;
  showScreen1 = false;
  dataDebts: any;
  isCheck = false;
  TableCheck = false;

  data: any[] = [];

  newDate = convertDate(new Date().toString());

  dataTableClone: {
    Code: any;
    Id: any;
    DateStart: any;
    DateEnd: any;
    Price: any;
    Check: any;
    Executor: any;
    Date: any;
    NearestActivities: any;
    PriceDiff: any;
    Index: any;
    AmountExclude: any;
    VATPrice: any;
    VAT: any;
    CheckPayDepartment: any;
    RentFileId: any;
    Type: any;
  }[] = [];

  dataTable: any;

  dataImport: any;

  role = this.commonService.CheckAccessKeyRole(AccessKey.SPECIAL_NOC_SELL_DEBT_BACK_STATUS_PAYMENT);

  SurplusBalance :  number = 0;

  Paid : number = 0;

  Diff :  number = 0;

  Total :   number = 0;
  
  constructor(
    private fb: FormBuilder,
    private message: NzMessageService,
    private modalSrv: NzModalService,
    private drawerRef: NzDrawerRef<string>,
    private commonService: CommonService,
    private promissoryRepository: PromissoryRepository,
    private settings: SettingsService,
    private debtsTableRepository: DebtsTableRepository,
    private debtsRepository: DebtsRepository,
    private drawerService: NzDrawerService
  ) {}

  get user(): any {
    return this.settings.user;
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [undefined],
      RentFileId: [this.record.Id],
      Code: [undefined],
      CustomerName: [undefined],
      Phone: [undefined],
      Address: [undefined],
      SurplusBalance: [undefined],
      Total: [undefined],
      Paid: [undefined],
      Diff: [undefined],
      DistrictId: [undefined],
      TypeBlockId: [undefined],
      UsageStatus: [undefined]
    });
    this.getDataDebts();
    this.setData();
    this.getDebts();

    console.log(this.role);
  }

  async submitForm() {
    this.drawerRef.close();
  }

  close(): void {
    this.drawerRef.close();
  }

  async getDataDebts() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query += ` and (Code.Contains("${this.record.CodeCH}")` + ` or Code.Contains("${this.record.CodeCN}"))`;
    const resp = await this.debtsRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataDebts = resp.data;
 
      this.dataDebts.forEach((e : any) => {
          this.SurplusBalance += e.SurplusBalance != null ? e.SurplusBalance :  0;
          this.Paid += e.Paid != null ? e.Paid :  0;
          this.Diff += e.Diff != null ? e.Diff :  0;
          this.Total += e.Total != null ? e.Total :  0;
      });
      if (this.dataDebts != undefined) {
        this.isCheck = true;
      }
    }
  }

  setData() {
    if (this.record) {
      if (this.record.CodeCH != null) {
        this.validateForm.get('Code')?.setValue(this.record.CodeCH);
      } else {
        this.validateForm.get('Code')?.setValue(this.record.CodeCN);
      }
      this.validateForm.get('CustomerName')?.setValue(this.record.CustomerName);
      this.validateForm.get('Address')?.setValue(this.record.Address);
      this.validateForm.get('Phone')?.setValue(this.record.Phone);
      this.validateForm.get('DistrictId')?.setValue(this.record.DistrictId);
      this.validateForm.get('TypeBlockId')?.setValue(this.record.TypeBlockId);
      this.validateForm.get('UsageStatus')?.setValue(this.record.UsageStatus);
    }
  }

  async getDebts() {
    const resp = await this.debtsTableRepository.groupData(this.validateForm.value.Code);
    this.dataTable = resp.data;
    let total = 0;
    this.dataTable.forEach((item: any) => {
      total = item.groupDataDebtsByDates.reduce((acc: any, data: any) => acc + data.Price, 0);
    });
    this.validateForm.get('Total')?.setValue(total);
  }

  async getPrice() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.order_by = 'CreatedAt Desc';
    paging.query += ` and (Code.Contains("${this.validateForm.value.Code}")` + ` or Code.Contains("${this.validateForm.value.Code}"))`;
    const resp = await this.promissoryRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      if (resp.data[0].Action == 1) {
        if (this.isCheck == false) {
          this.validateForm.get('SurplusBalance')?.setValue(this.validateForm.value.SurplusBalance + resp.data[0].Price);
        } else {
          this.SurplusBalance = this.SurplusBalance + resp.data[0].Price;
          this.validateForm.get('SurplusBalance')?.setValue(this.SurplusBalance);
        }
      } else {
        if (this.isCheck == false) {
          this.validateForm.get('SurplusBalance')?.setValue(this.validateForm.value.SurplusBalance - resp.data[0].Price);
        } else {
          this.SurplusBalance = this.SurplusBalance - resp.data[0].Price;
          this.validateForm.get('SurplusBalance')?.setValue(this.SurplusBalance);
        }
      }
    }
  }

  async check(env: number, j: number, i: number) {
    this.dataTable[env].groupDataDebtsByDates[j].Check = !this.dataTable[env].groupDataDebtsByDates[j].Check;
    if (
      (this.dataTable[env].groupDataDebtsByDates[j].Price <= this.validateForm.value.SurplusBalance &&
        this.dataTable[env].groupDataDebtsByDates[j].Check == true) ||
      (this.dataTable[env].groupDataDebtsByDates[j].Price <= this.SurplusBalance &&
        this.dataTable[env].groupDataDebtsByDates[j].Check == true)
    ) {
      this.dataTable[env].groupDataDebtsByDates[j].Executor = this.user.FullName;
      this.dataTable[env].groupDataDebtsByDates[j].Date = new Date().toISOString().slice(0, 10);
      this.dataTable[env].groupDataDebtsByDates[j].NearestActivities = 'Nộp tiền';
      this.dataTable[env].groupDataDebtsByDates[j].Check = true;
      this.dataTable[env].groupDataDebtsByDates[j].Index = env;
      this.dataTable[env].groupDataDebtsByDates[j].PriceDiff = 0;

      this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables = this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables.map(
        (x: any) => {
          return {
            Id: x.Id,
            Price: x.Price,
            Executor: this.user.FullName,
            Date: new Date().toISOString().slice(0, 10),
            NearestActivities: 'Nộp tiền',
            Check: true,
            Index: j,
            PriceDiff: 0,
            DateStart: x.DateStart,
            DateEnd: x.DateEnd,
            VAT: x.VAT,
            AmountExclude: x.Price / (1 + x.VAT / 100),
            VATPrice: (x.Price / (1 + x.VAT / 100)) * (x.VAT / 100),
            CheckPayDepartment: false,
            RentFileId: x.RentFileId,
            Code: x.Code,
            Type: x.Type
          };
        }
      );
      if (this.isCheck == false) {
        this.validateForm
          .get('SurplusBalance')
          ?.setValue(this.validateForm.value.SurplusBalance - this.dataTable[env].groupDataDebtsByDates[j].Price);
        this.validateForm.get('Paid')?.setValue(this.validateForm.value.Paid + this.dataTable[env].groupDataDebtsByDates[j].Price);
        this.validateForm.get('Diff')?.setValue(this.validateForm.value.Total - this.validateForm.value.Paid);
      } else {
        this.SurplusBalance = this.SurplusBalance - this.dataTable[env].groupDataDebtsByDates[j].Price;
        this.Paid = this.Paid + this.dataTable[env].groupDataDebtsByDates[j].Price;
        this.Diff = this.Total - this.Paid;

        this.validateForm.get('SurplusBalance')?.setValue(this.SurplusBalance);
        this.validateForm.get('Paid')?.setValue(this.Paid);
        this.validateForm.get('Diff')?.setValue(this.Diff);
      }
    } else if (this.dataTable[env].groupDataDebtsByDates[j].Check == false) {
      this.dataTable[env].groupDataDebtsByDates[j].Executor = this.user.FullName;
      this.dataTable[env].groupDataDebtsByDates[j].Date = new Date().toISOString().slice(0, 10);
      this.dataTable[env].groupDataDebtsByDates[j].NearestActivities = 'Bỏ nộp tiền';
      this.dataTable[env].groupDataDebtsByDates[j].Check = false;
      this.dataTable[env].groupDataDebtsByDates[j].Index = env;
      this.dataTable[env].groupDataDebtsByDates[j].CheckPayDepartment = false;
      this.dataTable[env].groupDataDebtsByDates[j].PriceDiff = this.dataTable[env].groupDataDebtsByDates[j].Price;

      this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables = this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables.map(
        (x: any) => {
          return {
            Id: x.Id,
            Executor: this.user.FullName,
            Date: new Date().toISOString().slice(0, 10),
            NearestActivities: 'Bỏ nộp tiền',
            Check: false,
            Index: j,
            CheckPayDepartment: false,
            PriceDiff: x.Price,
            Price: x.Price,
            DateStart: x.DateStart,
            DateEnd: x.DateEnd,
            VAT: x.VAT,
            AmountExclude: x.Price / (1 + x.VAT / 100),
            VATPrice: (x.Price / (1 + x.VAT / 100)) * (x.VAT / 100),
            RentFileId: x.RentFileId,
            Code: x.Code,
            Type: x.Type
          };
        }
      );

      if (this.isCheck == false) {
        this.validateForm
          .get('SurplusBalance')
          ?.setValue(this.validateForm.value.SurplusBalance + this.dataTable[env].groupDataDebtsByDates[j].Price);
        this.validateForm.get('Paid')?.setValue(this.validateForm.value.Paid - this.dataTable[env].groupDataDebtsByDates[j].Price);
        this.validateForm.get('Diff')?.setValue(this.validateForm.value.Total - this.validateForm.value.Paid);
      } else {
        this.SurplusBalance = this.SurplusBalance + this.dataTable[env].groupDataDebtsByDates[j].Price;
        this.Paid = this.Paid - this.dataTable[env].groupDataDebtsByDates[j].Price;
        this.Diff = this.Total - this.Paid;

        this.validateForm.get('SurplusBalance')?.setValue(this.SurplusBalance);
        this.validateForm.get('Paid')?.setValue(this.Paid);
        this.validateForm.get('Diff')?.setValue(this.Diff);
      }
    }
    for (let item = 0; item < this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables.length; item++) {
      console.log(this.dataTable[env].groupDataDebtsByDates[j].Price);
      const resp = await this.debtsTableRepository.update(this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables[item],this.dataTable[env].groupDataDebtsByDates[j].Price);
    }
  }

  async CheckPayDepartment(env: any, j: number, i: number) {
    this.dataTable[env].groupDataDebtsByDates[j].CheckPayDepartment = !this.dataTable[env].groupDataDebtsByDates[j].CheckPayDepartment;

    this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables = this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables.map(
      (x: any) => {
        return {
          Id: x.Id,
          Executor: x.Executor,
          Date: x.Date,
          NearestActivities: x.NearestActivities,
          Check: x.Check,
          Index: x.Index,
          CheckPayDepartment: this.dataTable[env].groupDataDebtsByDates[j].CheckPayDepartment,
          PriceDiff: 0,
          Price: x.Price,
          DateStart: x.DateStart,
          DateEnd: x.DateEnd,
          VAT: x.VAT,
          AmountExclude: x.Price / (1 + x.VAT / 100),
          VATPrice: (x.Price / (1 + x.VAT / 100)) * (x.VAT / 100),
          RentFileId: x.RentFileId,
          Code: x.Code,
          Type: x.Type
        };
      }
    );
    for (let item = 0; item < this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables.length; item++) {
      const resp = await this.debtsTableRepository.update(this.dataTable[env].groupDataDebtsByDates[j].groupDebtTables[item],0);
    }
  }

  Payment() {
    let total = 0;
    if (this.isCheck == false) {
      total = this.validateForm.value.SurplusBalance;
    } else {
      total = this.SurplusBalance;
    }
    this.modalSrv.create({
      nzTitle: `Phiếu thu cho mã định danh "${this.validateForm.value.Code}"`,
      nzContent: PaymentComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        code: this.validateForm.value.Code
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thêm phiếu thu cho mã định danh ${this.validateForm.value.Code} thành công!`);
        this.getPrice();
      }
    });
  }

  Receipts() {
    let total = 0;
    if (this.isCheck == false) {
      total = this.validateForm.value.SurplusBalance;
    } else {
      total = this.SurplusBalance;
    }
    this.modalSrv.create({
      nzTitle: `Phiếu chi cho mã định danh "${this.validateForm.value.Code}"`,
      nzContent: ReceiptsComponent,
      nzComponentParams: {
        SurplusBalance: total,
        code: this.validateForm.value.Code
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thêm phiếu chi cho mã định danh ${this.validateForm.value.Code} thành công!`);
        this.getPrice();
      }
    });
  }

  ImportDebt() {
    const drawerRef = this.drawerService.create<ImportDebtComponent>({
      nzTitle: 'Thêm dữ liệu công nợ',
      nzWidth: '85vw',
      nzContent: ImportDebtComponent,
      nzPlacement: 'right',
      nzContentParams: {
        code: this.validateForm.value.Code,
        RentFileId: this.validateForm.value.RentFileId
      }
    });
  }

  async dowload() {
    const resp = await this.debtsTableRepository.Debts(this.validateForm.value.Code, this.dataTable);
  }
}
