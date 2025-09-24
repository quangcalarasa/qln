import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167KiosRepository } from 'src/app/infrastructure/repositories/md167kios.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import GetByPageModel, { GetByPageMd167HouseModel } from 'src/app/core/models/get-by-page-model';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { KiosStatus } from 'src/app/shared/utils/consts';
import { NzModalRef, NzModalService } from 'ng-zorro-antd/modal';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import { Md167LandTaxRepository } from 'src/app/infrastructure/repositories/md167landtax.repository';
import { roundIfDecimal } from 'src/app/shared/utils/common';

@Component({
  selector: 'app-addorupdate',
  templateUrl: './addorupdate.component.html',
  styles: [
  ]
})
export class AddorupdateMd167KiosComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  valueTaxNN = 0;
  landtax: any;
  landTaxData: any;
  lstApa: any;
  kiosStatus = KiosStatus;
  TypeQD = TypeQD;
  @Input() record: any;
  @Input() houseSelect: any;

  myForm = new FormGroup({
    IsPayTax: new FormControl(false)
  });

  constructor(private fb: FormBuilder,
    private md167KiosRepository: Md167KiosRepository,
    private md167HouseRepository: Md167HouseRepository,
    private md167LandTaxRepository: Md167LandTaxRepository,
    private modalSrv: NzModalService,
    private modal: NzModalRef,
    private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.getInfoApartment();
    console.log(this.houseSelect);

    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : 0],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      HouseNumber: [this.record ? this.record.HouseNumber : undefined, [Validators.required]],
      UseFloorPb: [this.record ? this.record.UseFloorPb : undefined, [Validators.required]],
      UseFloorPr: [this.record ? this.record.UseFloorPr : undefined, [Validators.required]],
      KiosStatus: [this.record ? this.record.KiosStatus : undefined, [Validators.required]],
      index: [this.record ? this.record.index : undefined, []],
      TaxNN: [{ value: this.record ? this.record.TaxNN : 0, disabled: true }, []],
      Note: [this.record ? this.record.Note : undefined, []],
      IsPayTax: [this.record ? this.record.IsPayTax : false, []],
      TypeHouse: 3,
    });

  }
  checkbox() {
    console.log(this.validateForm.value.IsPayTax);

    this.validateForm.get('IsPayTax')?.setValue(!this.validateForm.value.IsPayTax)
  }
  async getInfoApartment() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.query = `Id=${this.houseSelect.LandTaxRate}`;
    paging.page_size = 0;
    paging.select = 'TypeArea,Tax';

    const resp = await this.md167LandTaxRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.landTaxData = resp.data[0];
      this.landtax = this.landTaxData.Tax;

    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  change() {
    this.calcTaxNN()
  }
  close(): void {
    this.modal.close();
  }
  check() {
    console.log(this.houseSelect);

  }

  calcTaxNN() {
    this.valueTaxNN = 0;
    if (this.validateForm != undefined && this.houseSelect != undefined && this.houseSelect.ApaValue != 0) {
      let k = this.houseSelect.ApaValue;
      let landprice = this.houseSelect.UnitPriceValue;
      let tax = this.houseSelect.ApaTax;
      let floor = this.validateForm.value.UseFloorPb / k + this.validateForm.value.UseFloorPr;
      console.log(k+"  "+landprice+" "+tax+"  "+floor );
      
      this.valueTaxNN = roundIfDecimal((floor * k * landprice * this.landtax / 100)
        - (floor * k * landprice * this.landtax / 100) * tax / 100, 3);
    }

    this.validateForm.get('TaxNN')?.setValue(this.valueTaxNN);
  }
  async submitForm() {
    this.validateForm.get('TaxNN')?.setValue(this.valueTaxNN);
    this.modal.triggerOk();
  }

}