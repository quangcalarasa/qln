import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { DefaultCoefficientRepository } from '../../../../infrastructure/repositories/default-coefficient.repositories';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { TypeCoefficient, TypeQD } from '../../../../shared/utils/consts';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { RentFileBCTRepository } from 'src/app/infrastructure/repositories/rent-File-BCT.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { ConversionRepository } from 'src/app/infrastructure/repositories/conversion.repository';
import { DebtsTableRepository } from 'src/app/infrastructure/repositories/DebtsTable.repository';
import { async } from 'rxjs';
import { DiscountCoefficientRepository } from 'src/app/infrastructure/repositories/discount-coefficient.repositories';

@Component({
  selector: 'app-arrears',
  templateUrl: './arrears.component.html',
  styles: []
})
export class ArrearsComponent implements OnInit {
  @Input() RentFileId: NzSafeAny;
  @Input() record: NzSafeAny;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;
  data: any[] = [];
  Check: boolean = false;
  invalid_tableItemRef = true;

  paging: GetByPageModel = new GetByPageModel();

  TypeCoefficient: { [key: number]: string } = TypeCoefficient;
  TypeQD = TypeQD;

  dataDefaultGroup: any[] = [];

  unit_price_data: any[] = [];

  dataRentFile: any[] = [];

  dataConversion: any[] = [];

  dataConversion09: any[] = [];

  dataConversion1753: any[] = [];

  data_GetWorkSheet1753: any[] = [];

  data_GetWorkSheet09: any[] = [];

  data_GetWorkSheet22: any[] = [];
  check: boolean = false;

  date1753_End = '2007-01-18';

  date09_Start = '2017-01-19';
  date09_End = '2018-07-11';

  date22_Start = '2018-07-12';

  use: boolean = false;

  isCheck: boolean = false;

  dataBCT: any;

  checkBCT: boolean = false;

  nzFormat = 'dd/ MM/ yyyy';
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

  dataDiscountCoefficient: any;

  constructor(
    private defaultCoefficientRepository: DefaultCoefficientRepository,
    private message: NzMessageService,
    private unitPriceRepository: UnitPriceRepository,
    private fb: FormBuilder,
    private rentFileRepository: RentFileRepository,
    private drawerRef: NzDrawerRef<string>,
    private rentFileBCTRepository: RentFileBCTRepository,
    private conversionRepository: ConversionRepository,
    private debtsTableRepository: DebtsTableRepository,
    private discountCoefficientRepository: DiscountCoefficientRepository
  ) {
    this.getDataDiscountCoefficient();
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [undefined],
      RentFileId: [this.RentFileId],
      CodeHd: [undefined],
      Code: [undefined],
      CustomerName: [undefined],
      DateStart: [this.record ? convertDate(this.record.DateStart) : undefined, [Validators.required]],
      DateEnd: [this.record ? convertDate(this.record.DateEnd) : undefined, [Validators.required]],
      Area: [this.record ? this.record.Area : undefined, [Validators.required]],
      TypeBCT: [2],
      groupData: [this.record ? this.record.groupData : [], []],
      DiscountCoefficientId: []
    });
    this.getUnitPrice();
    this.getDataRentFile();
    this.getDataConversion22();
    this.getDataConversion1753();
    this.getDataConversion09();
    if (this.record == undefined) {
      this.getDeaultCoefficientGroup(0);
    }
    if (this.record != undefined) {
      this.GetById(this.record.Id);
    }
  }

  async getDataDiscountCoefficient() {
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.discountCoefficientRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataDiscountCoefficient = [...resp.data];
    }
  }

  async submitForm() {
    this.loading = true;
    if (this.isCheck == false) {
      this.validateForm.get('groupData')?.setValue(this.dataDefaultGroup);
      if (this.record != undefined) {
        this.validateForm.get('Id')?.setValue(this.record.Id);
      }
      let data = { ...this.validateForm.value };
      delete data.Id;
      const resp = data.Id ? await this.rentFileBCTRepository.update(data) : await this.rentFileBCTRepository.addNew(data);
      if (resp.meta?.error_code == 200) {
        this.loading = false;
        this.message.create('success', `Thêm mới thành công!`);
        this.GetById(resp.data.Id);
        this.checkBCT = true;
        this.Check == false;
      } else {
        this.loading = false;
        this.message.create('success', `Thêm mới thất bại!`);
      }
    } else {
      this.validateForm.get('groupData')?.setValue(this.dataDefaultGroup);
      this.validateForm.get('Id')?.setValue(this.dataBCT.Id);
      let data = { ...this.validateForm.value };

      const resp = await await this.rentFileBCTRepository.update(data);
      if (resp.meta?.error_code == 200) {
        this.loading = false;
        this.message.create('success', `Sửa bảng chiết tính thành công!`);
        this.GetById(resp.data.Id);
        this.checkBCT = false;
      } else {
        this.loading = false;
        this.message.create('success', `Sửa bảng chiết tính thất bại!`);
      }
    }
  }

  async setDataBCT1753() {
    this.dataTableClone = this.data_GetWorkSheet1753.map((x: any) => {
      return {
        Id: undefined,
        Index: undefined,
        DateStart: x.DateStart,
        DateEnd: x.DateEnd,
        Price: x.totalPrice,
        Check: false,
        Executor: undefined,
        Date: null,
        NearestActivities: undefined,
        PriceDiff: x.totalPrice,
        VAT: x.VAT,
        AmountExclude: x.totalPrice / (1 + x.VAT / 100),
        VATPrice: (x.totalPrice / (1 + x.VAT / 100)) * (x.VAT / 100),
        CheckPayDepartment: false,
        RentFileId: this.RentFileId,
        Code: this.validateForm.value.Code,
        Type: 2
      };
    });
    const resp = await this.debtsTableRepository.Post(this.dataTableClone);
  }

  async setData09() {
    this.dataTableClone = this.data_GetWorkSheet09.map((x: any) => {
      return {
        Id: undefined,
        Index: undefined,
        DateStart: x.DateStart,
        DateEnd: x.DateEnd,
        Price: x.totalPrice,
        Check: false,
        Executor: undefined,
        Date: null,
        NearestActivities: undefined,
        PriceDiff: x.totalPrice,
        VAT: x.VAT,
        AmountExclude: x.totalPrice / (1 + x.VAT / 100),
        VATPrice: (x.totalPrice / (1 + x.VAT / 100)) * (x.VAT / 100),
        CheckPayDepartment: false,
        RentFileId: this.RentFileId,
        Code: this.validateForm.value.Code,
        Type: 2
      };
    });
    const resp = await this.debtsTableRepository.Post(this.dataTableClone);
  }

  async setData22() {
    this.dataTableClone = this.data_GetWorkSheet22.map((x: any) => {
      return {
        Id: undefined,
        Index: undefined,
        DateStart: x.DateStart,
        DateEnd: x.DateEnd,
        Price: x.totalPrice,
        Check: false,
        Executor: undefined,
        Date: null,
        NearestActivities: undefined,
        PriceDiff: x.totalPrice,
        VAT: x.VAT,
        AmountExclude: x.totalPrice / (1 + x.VAT / 100),
        VATPrice: (x.totalPrice / (1 + x.VAT / 100)) * (x.VAT / 100),
        CheckPayDepartment: false,
        RentFileId: this.RentFileId,
        Code: this.validateForm.value.Code,
        Type: 2
      };
    });
    const resp = await this.debtsTableRepository.Post(this.dataTableClone);
  }

  close(): void {
    this.drawerRef.close();
  }

  SetData() {
    if (this.dataRentFile) {
      this.validateForm.get('CodeHd')?.setValue(this.dataRentFile[0].Code);
      if (this.dataRentFile[0].CodeCN != null) {
        this.validateForm.get('Code')?.setValue(this.dataRentFile[0].CodeCN);
      } else {
        this.validateForm.get('Code')?.setValue(this.dataRentFile[0].CodeCH);
      }
      this.validateForm.get('CustomerName')?.setValue(this.dataRentFile[0].CustomerName);
    }
  }

  async getDataRentFile() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id.ToString().Contains("${this.record.Id}")`;
    const resp = await this.rentFileRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataRentFile = [...resp.data];
    }
    this.SetData();
  }

  async GetById(env: any) {
    const resp = await this.rentFileBCTRepository.GetById(env);
    if (resp.meta?.error_code == 200) {
      this.dataBCT = resp.data;
      this.validateForm.get('DiscountCoefficientId')?.setValue(this.dataBCT.DiscountCoefficientId);
      this.isCheck = true;
    }
    if (this.dataBCT != undefined) {
      this.getDeaultCoefficientGroup(this.dataBCT.Id);
    } else {
      this.getDeaultCoefficientGroup(0);
    }
    this.getWorkSheet(env);
  }

  async getDeaultCoefficientGroup(env: any) {
    const resp = await this.rentFileBCTRepository.GroupedData(env, this.validateForm.value.TypeBCT);
    if (resp.meta?.error_code == 200) {
      resp.data.forEach((item: any) => {
        if (item.childDefaultCoefficients) {
          item.childDefaultCoefficients = item.childDefaultCoefficients.map((item: any) => {
            item.DoApply = convertDate(item.DoApply);
            delete item.Id;
            return item;
          });
        }
      });
      this.dataDefaultGroup = [...resp.data];
    }
  }

  async getUnitPrice() {
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.unitPriceRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.unit_price_data = resp.data;
    }
  }

  async genValue(env: any, row: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${env}`;
    const resp = await this.defaultCoefficientRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      let res = resp.data.find((x: any) => x.Id == env);
      row.Value = res.Value;
      row.UnitPriceId = res.UnitPriceId;
    }
  }

  addRow(env: any) {
    if (this.dataDefaultGroup[env].childDefaultCoefficients == undefined) {
      this.dataDefaultGroup[env].childDefaultCoefficients = [];
    }
    const newRow = {
      CoefficientId: this.dataDefaultGroup[env].CoefficientId,
      DoApply: null,
      defaultCoefficientsId: null,
      Value: null,
      UnitPriceId: null,
      edit: true,
      Type: 2,
      index: this.dataDefaultGroup[env].childDefaultCoefficients.length + 1
    };
    this.dataDefaultGroup[env].childDefaultCoefficients.push(newRow);
    this.dataDefaultGroup = [...this.dataDefaultGroup];
  }

  saveRow(index: number, evt: any) {
    const saveRow = {
      CoefficientId: this.dataDefaultGroup[index].childDefaultCoefficients[evt].CoefficientId,
      DoApply: this.dataDefaultGroup[index].childDefaultCoefficients[evt].DoApply,
      defaultCoefficientsId: this.dataDefaultGroup[index].childDefaultCoefficients[evt].defaultCoefficientsId,
      Value: this.dataDefaultGroup[index].childDefaultCoefficients[evt].Value,
      UnitPriceId: this.dataDefaultGroup[index].childDefaultCoefficients[evt].UnitPriceId,
      Type: this.dataDefaultGroup[index].childDefaultCoefficients[evt].Type,
      edit: false
    };

    this.dataDefaultGroup[index].childDefaultCoefficients[evt] = saveRow;
    this.dataDefaultGroup = [...this.dataDefaultGroup];

    this.Check = true;
    this.message.create('success', `Lưu  ${this.getTypeCoefficientName(index + 1)} thành công!`);
  }

  delete(env: any, i: any) {
    if (this.dataDefaultGroup[env]?.childDefaultCoefficients) {
      this.dataDefaultGroup[env].childDefaultCoefficients.splice(i, 1);
    }
    this.Check = true;
    this.message.create('success', `Xóa  ${this.getTypeCoefficientName(env + 1)} thành công!`);
  }

  edit(env: any, i: any) {
    const newRow = {
      CoefficientId: this.dataDefaultGroup[env].childDefaultCoefficients[i].CoefficientId,
      DoApply: this.dataDefaultGroup[env].childDefaultCoefficients[i].DoApply,
      Value: this.dataDefaultGroup[env].childDefaultCoefficients[i].Value,
      defaultCoefficientsId: this.dataDefaultGroup[env].childDefaultCoefficients[i].defaultCoefficientsId,
      UnitPriceId: this.dataDefaultGroup[env].childDefaultCoefficients[i].UnitPriceId,
      Type: this.dataDefaultGroup[env].childDefaultCoefficients[i].Type,
      edit: true
    };
    this.dataDefaultGroup[env].childDefaultCoefficients[i] = newRow;
    this.dataDefaultGroup = [...this.dataDefaultGroup];
  }

  cancle(env: any, i: any) {
    if (this.dataDefaultGroup[env].childDefaultCoefficients[i].DoApply == null) {
      this.dataDefaultGroup[env].childDefaultCoefficients.splice(i, 1);
    } else {
      const newRow = {
        CoefficientId: this.dataDefaultGroup[env].childDefaultCoefficients[i].CoefficientId,
        DoApply: this.dataDefaultGroup[env].childDefaultCoefficients[i].DoApply,
        Value: this.dataDefaultGroup[env].childDefaultCoefficients[i].Value,
        defaultCoefficientsId: this.dataDefaultGroup[env].childDefaultCoefficients[i].defaultCoefficientsId,
        UnitPriceId: this.dataDefaultGroup[env].childDefaultCoefficients[i].UnitPriceId,
        Type: this.dataDefaultGroup[env].childDefaultCoefficients[i].Type,
        edit: false
      };

      this.dataDefaultGroup[env].childDefaultCoefficients[i] = newRow;
      this.dataDefaultGroup = [...this.dataDefaultGroup];
    }
  }

  async getDataConversion22() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TypeQD=22`;
    paging.order_by = `CoefficientName`;
    const resp = await this.conversionRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataConversion = [...resp.data];
    }
  }

  async getDataConversion1753() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TypeQD=1753`;
    paging.order_by = `CoefficientName`;
    const resp = await this.conversionRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataConversion1753 = [...resp.data];
    }
  }

  async getDataConversion09() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TypeQD=9`;
    paging.order_by = `CoefficientName`;
    const resp = await this.conversionRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataConversion09 = [...resp.data];
    }
  }

  async getWorkSheet(env: any) {
    let total = 0;

    ///API gọi BCT QD-1753
    if (this.validateForm.value.DateEnd < this.date1753_End) {
      const resp1753 = await this.rentFileBCTRepository.GetExcelTable1753TT(this.RentFileId, env);
      if (resp1753.meta?.error_code == 200) {
        this.data_GetWorkSheet1753 = resp1753.data;
        this.data_GetWorkSheet1753.forEach((item: any) => {
          total += item.bCTs.reduce((acc: any, data: any) => acc + data.PriceRent, 0);
        });
      }
    }

    ///API gọi BCT QD-09
    if (this.date09_Start <= this.validateForm.value.DateStart && this.validateForm.value.DateStart < this.date09_End) {
      const resp09 = await this.rentFileBCTRepository.GetExcelTable09TT(this.RentFileId, env);
      if (resp09.meta?.error_code == 200) {
        this.data_GetWorkSheet09 = resp09.data;
        this.data_GetWorkSheet09.forEach((item: any) => {
          total += item.bCTs.reduce((acc: any, data: any) => acc + data.PriceRent, 0);
        });
      }
    }

    ///API gọi BCT QD-22
    if (this.validateForm.value.DateStart >= this.date22_Start) {
      const resp22 = await this.rentFileBCTRepository.GetExcelTableBCT22TT(this.RentFileId, env);
      if (resp22.meta?.error_code == 200) {
        this.data_GetWorkSheet22 = resp22.data;
        this.data_GetWorkSheet22.forEach((item: any) => {
          total += item.bCTs.reduce((acc: any, data: any) => acc + data.PriceRent, 0);
        });
      }
    }

    ///API gọi BCT QD-09 && QD-1753
    if (this.validateForm.value.DateStart < this.date1753_End && this.validateForm.value.DateEnd < this.date09_End) {
      const resp1753 = await this.rentFileBCTRepository.GetExcelTable1753TT(this.RentFileId, env);
      if (resp1753.meta?.error_code == 200) {
        this.data_GetWorkSheet1753 = resp1753.data;
      }
      const resp09 = await this.rentFileBCTRepository.GetExcelTable09TT(this.RentFileId, env);
      if (resp09.meta?.error_code == 200) {
        this.data_GetWorkSheet09 = resp09.data;
      }
    }

    ///API gọi BCT QD-09 && QD-22
    if (
      this.validateForm.value.DateStart > this.date1753_End &&
      this.validateForm.value.DateStart < this.date09_End &&
      this.validateForm.value.DateEnd > this.date22_Start
    ) {
      const resp09 = await this.rentFileBCTRepository.GetExcelTable09TT(this.RentFileId, env);
      if (resp09.meta?.error_code == 200) {
        this.data_GetWorkSheet09 = resp09.data;
      }

      const resp22 = await this.rentFileBCTRepository.GetExcelTableBCT22TT(this.RentFileId, env);
      if (resp22.meta?.error_code == 200) {
        this.data_GetWorkSheet22 = resp22.data;
      }
    }

    ///API gọi BCT QD-09 && QD-22 && QD-1753
    if (this.validateForm.value.DateStart < this.date1753_End && this.validateForm.value.DateEnd > this.date22_Start) {
      const resp1753 = await this.rentFileBCTRepository.GetExcelTable1753TT(this.RentFileId, env);
      if (resp1753.meta?.error_code == 200) {
        this.data_GetWorkSheet1753 = resp1753.data;
      }

      const resp09 = await this.rentFileBCTRepository.GetExcelTable09TT(this.RentFileId, env);
      if (resp09.meta?.error_code == 200) {
        this.data_GetWorkSheet09 = resp09.data;
      }

      const resp22 = await this.rentFileBCTRepository.GetExcelTableBCT22TT(this.RentFileId, env);
      if (resp22.meta?.error_code == 200) {
        this.data_GetWorkSheet22 = resp22.data;
      }
    }

    if (this.checkBCT == true) {
      if (this.data_GetWorkSheet1753.length != 0) {
        this.setDataBCT1753();
      }

      if (this.data_GetWorkSheet09.length != 0) {
        this.setData09();
      }

      if (this.data_GetWorkSheet22.length != 0) {
        this.setData22();
      }
    }
  }

  getTypeCoefficientName(CoefficientId: any): string {
    return this.TypeCoefficient[CoefficientId];
  }

  getConversionCode(CoefficientName: number) {
    let Conversion = this.dataConversion.find(x => x.CoefficientName == CoefficientName);
    return Conversion ? Conversion.Code : '';
  }

  getConversionCode1753(CoefficientName: number) {
    let Conversion = this.dataConversion1753.find(x => x.CoefficientName == CoefficientName);
    return Conversion ? Conversion.Code : '';
  }

  getConversionCode09(CoefficientName: number) {
    let Conversion = this.dataConversion09.find(x => x.CoefficientName == CoefficientName);
    return Conversion ? Conversion.Code : '';
  }

  button() {
    this.check = true;
  }

  HDSD() {
    this.use = !this.use;
  }

  dowload() {
    this.rentFileBCTRepository.exportReportTT(this.RentFileId, this.dataBCT.Id);
  }
}
