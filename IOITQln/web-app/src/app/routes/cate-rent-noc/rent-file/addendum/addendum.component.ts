import { Component, Input, OnInit, ChangeDetectorRef, ViewChild, SimpleChanges } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';

import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';

@Component({
  selector: 'app-addendum',
  templateUrl: './addendum.component.html',
  styles: []
})
export class AddendumComponent implements OnInit {
  @Input() record: NzSafeAny;
  @Input() parent : NzSafeAny;
  @Input() typehouse_data: NzSafeAny;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;

  customer_data: any[] = [];
  customer_data_filter: any[] = [];
  block_data: any[] = [];
  block_Filter_data: any[] = [];
  data_filter_block: any[] = [];

  apartment_data: any[] = [];
  data_filter_apartment: any[] = [];
  nzFormat = 'dd/ MM/ yyyy';
  level_data = LevelBlock;
  TypeReportApply = TypeReportApply;
  Id: any;

  constructor(
    private fb: FormBuilder,
    private rentFileRepository: RentFileRepository,
    private modal: NzModalRef,
    private customerRepository: CustomerRepository,
    private blockRepository: BlockRepository,
    private apartmentRepository: ApartmentRepository
  ) {}

  ngOnInit(): void {
      this.validateForm = this.fb.group({
        Id: [this.record ? this.record.Id : undefined],
        CustomerId: [this.record ? this.record.CustomerId : this.parent.CustomerId],
        RentBlockId: [this.record ? this.record.RentBlockId : this.parent.RentBlockId],
        RentApartmentId: [this.record ? this.record.RentApartmentId : this.parent.RentApartmentId],
        FileStatus: [this.record ? this.record.FileStatus : this.parent.FileStatus],
        Code: [this.record ? this.record.Code : this.parent.Code],
        CodeHS: [this.record ? this.record.CodeHS : this.parent.CodeHS],
        DateHD: [this.record ?convertDate(this.record.DateHD) : undefined],
        Dob: [this.record ? this.record.Dob : this.parent.Dob],
        CodeKH: [this.record ? this.record.CodeKH : this.parent.CodeKH],
        AddressKH: [this.record ? this.record.AddressKH : this.parent.AddressKH],
        Phone: [this.record ? this.record.Phone : this.parent.Phone],
        TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : this.parent.TypeReportApply.toString()],
        TypeBlockId: [this.record ? this.record.TypeBlockId : this.parent.TypeBlockId],
        CampusArea: [this.record ? this.record.CampusArea : this.parent.CampusArea],
        fullAddressCN: [this.record ? this.record.fullAddressCN : this.parent.fullAddressCN],
        ConstructionAreaValue: [this.record ? this.record.ConstructionAreaValue : this.parent.ConstructionAreaValue],
        UseAreaValueCN: [this.record ? this.record.UseAreaValueCN : this.parent.UseAreaValueCN],
        CodeCN: [this.record ? this.record.CodeCN : this.parent.CodeCN],
        CodeCH: [this.record ? this.record.CodeCH : this.parent.CodeCH],
        UseAreaValueCH: [this.record ? this.record.UseAreaValueCH : this.parent.UseAreaValueCH],
        fullAddressCH: [this.record ? this.record.fullAddressCH : this.parent.fullAddressCH],
        DistrictId: [this.record ? this.record.DistrictId : this.parent.DistrictId],
        UsageStatus: [this.record ? this.record.UsageStatus : this.parent.UsageStatus],
        Month: [this.record ? this.record.Month : undefined,[Validators.required, Validators.max(60)]],
        Type: [this.record ? this.record.Type : 2],
        ParentId: [this.record ? this.record.ParentId : this.parent.Id, []] //Id Hợp đồng cha
      });

    this.getDataCustomer();
    this.getDataBlock();
    this.getDataApartment();
    if (this.validateForm.value.RentBlockId) {
      this.GetBlockDataFilter(this.validateForm.value.RentBlockId);
    }
    if (this.validateForm.value.RentApartmentId) {
      this.GetApartmentDataFilter(this.validateForm.value.RentApartmentId);
    }
    this.getDate();
    if (this.record.Type == 2) {
      this.validateForm.get('Id')?.setValue(this.record.Id);
      this.validateForm.get('Month')?.setValue(this.record.Month);
      this.validateForm.get('DateHD')?.setValue(convertDate(this.record.DateHD));
    }
  }

  getDate() {
    let dateHD = this.parent.DateHD;
    if (!(dateHD instanceof Date)) {
      dateHD = new Date(dateHD);
    }
    if (dateHD instanceof Date) {
      const newDate = new Date(dateHD.getTime());
      newDate.setMonth(newDate.getMonth() + 60);
      newDate.setDate(newDate.getDate() + 1);

      const newDate1 = newDate.toISOString().split('.')[0];
      this.validateForm.get('DateHD')?.setValue(convertDate(newDate1));
    }
  }

  async submitForm() {
    let data = { ...this.validateForm.value };
    try {
      this.loading = true;
      const resp = data.Id ? await this.rentFileRepository.update(data) : await this.rentFileRepository.addNew(data);
      if (resp.meta?.error_code == 200) {
        this.modal.triggerOk();
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  close(): void {
    this.modal.close();
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2;
  };

  async getDataCustomer() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Code,FullName,Phone,Address,Dob';

    const resp = await this.customerRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.customer_data = [...resp.data];
    }
  }

  SetDataCustomer() {
    if (this.customer_data_filter) {
      this.validateForm.get('CodeKH')?.setValue(this.customer_data_filter[0].Code);
      this.validateForm.get('Phone')?.setValue(this.customer_data_filter[0].Phone);
      this.validateForm.get('Dob')?.setValue(convertDate(this.customer_data_filter[0].Dob));
      this.validateForm.get('AddressKH')?.setValue(this.customer_data_filter[0].Address);
    }
  }

  async GetCustomerFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    paging.select = 'Id,Code,FullName,Phone,Address,Dob';
    const resp = await this.customerRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.customer_data_filter = [...resp.data];
    }
    this.SetDataCustomer();
  }

  async getDataBlock() {
    let TypeReportApply = this.validateForm.value.TypeReportApply;
    let TypeBlockId = this.validateForm.value.TypeBlockId;
    if (TypeReportApply && TypeBlockId) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `TypeReportApply=${TypeReportApply} AND TypeBlockId=${TypeBlockId} AND TypeBlockEntity=${2} `;
      const resp = await this.blockRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.block_data = [...resp.data];
      }
    }
  }

  SetDataBlock() {
    if (this.data_filter_block) {
      this.validateForm.get('CampusArea')?.setValue(this.data_filter_block[0].CampusArea);
      this.validateForm.get('fullAddressCN')?.setValue(this.data_filter_block[0].FullAddress);
      this.validateForm.get('ConstructionAreaValue')?.setValue(this.data_filter_block[0].ConstructionAreaValue);
      this.validateForm.get('UseAreaValueCN')?.setValue(this.data_filter_block[0].UseAreaValue);
      this.validateForm.get('CodeCN')?.setValue(this.data_filter_block[0].Code);
    }
  }

  async GetBlockDataFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    const resp = await this.blockRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.block_Filter_data = [...resp.data[0].levelBlockMaps];
      this.data_filter_block = [...resp.data];
    }
    this.SetDataBlock();
  }

  SetDataApartment() {
    if (this.apartment_data) {
      this.validateForm.get('fullAddressCH')?.setValue(this.apartment_data[0].Address);
      this.validateForm.get('UseAreaValueCH')?.setValue(this.apartment_data[0].UseAreaValue);
      this.validateForm.get('CodeCH')?.setValue(this.apartment_data[0].Code);
    }
  }

  async getDataApartment() {
    let BlockId = this.validateForm.value.RentBlockId;
    if (BlockId) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `BlockId=${BlockId}`;
      const resp = await this.apartmentRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.apartment_data = [...resp.data];
      }
    }
  }

  async GetApartmentDataFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    const resp = await this.apartmentRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.data_filter_apartment = [...resp.data];
    }
    this.SetDataApartment();
  }
}
