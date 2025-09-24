import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { RentBlockDetailComponent } from 'src/app/routes/cate-rent-noc/rent-block/rent-block-detail/rent-block-detail.component';
import { TypeBlockEntityEnum, TypeApartmentEntityEnum, TypeReportApplyEnum, AccessKey } from 'src/app/shared/utils/enums';
import { LevelBlock, TypeReportApply, UsageStatus } from 'src/app/shared/utils/consts';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TypeHouse } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-rent-apartment',
  templateUrl: './add-or-update-rent-apartment.component.html'
})
export class AddOrUpdateRentApartmentComponent implements OnInit {
  @ViewChild('rentBlockDetailComponent') rentBlockDetailComponent!: RentBlockDetailComponent;
  @Input() record: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;
  @Input() blocks: any[] = [];
  @Input() editHistory: NzSafeAny;
  @Input() isViewRecord?: boolean;

  role = this.commonService.CheckAccessKeyRole(AccessKey.RENT_APARTMENT);
  validateForm!: FormGroup;
  loading: boolean = false;
  invalidRentBlockDetail: boolean = false;

  usageStatus_data = UsageStatus;
  TypeHouse = TypeHouse;
  newDate = convertDate(new Date().toString());
  date: any;
  block: any;
  customers: any[] = [];

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private customerRepository: CustomerRepository,
    private apartmentRepository: ApartmentRepository,
    private blockRepository: BlockRepository,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      BlockId: [this.record ? this.record.BlockId : undefined, [Validators.required]],
      Code: [this.record ? this.record.Code : undefined, []],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
      UseAreaValue: [this.record ? this.record.UseAreaValue : undefined, []],
      apartmentDetails: [
        this.record
          ? this.record.apartmentDetails.map((item: any) => {
              item.Level = item.Level.toString();
              item.DisposeTime = item.DisposeTime ? convertDate(item.DisposeTime) : undefined;
              return item;
            })
          : [],
        []
      ],
      TypeApartmentEntity: [this.record ? this.record.TypeApartmentEntity : TypeApartmentEntityEnum.APARTMENT_RENT, []],
      EstablishStateOwnership: [this.record ? this.record.EstablishStateOwnership : undefined, []],
      Dispute: [this.record ? this.record.Dispute : undefined, []],
      Blueprint: [this.record ? this.record.Blueprint : undefined, []],
      CustomerId: [this.record ? this.record.CustomerId : undefined, []],
      UsageStatus: [this.record ? (this.record.UsageStatus ? this.record.UsageStatus.toString() : undefined) : undefined, []],
      UsageStatusNote: [this.record ? this.record.UsageStatusNote : undefined, []],
      UseAreaNote1: [this.record ? this.record.UseAreaNote1 : undefined, []],
      UseAreaNote: [this.record ? this.record.UseAreaNote : undefined, []],
      DateRecord: [this.record ? convertDate(this.record.DateRecord) : new Date().toISOString().slice(0, 10), []],
      TakeOver: [this.record ? this.record.TakeOver : undefined, []],
      TypeHouse: [this.record ? this.record.TypeHouse.toString() : undefined],
      DateApply: [this.record ? convertDate(this.record.DateApply) : undefined],
      CodeEstablishStateOwnership: [this.record ? this.record.CodeEstablishStateOwnership : undefined],
      DateEstablishStateOwnership: [this.record ? convertDate(this.record.DateEstablishStateOwnership) : undefined],
      NameBlueprint: [this.record ? this.record.NameBlueprint : undefined],
      DateBlueprint: [this.record ? convertDate(this.record.DateBlueprint) : undefined]
    });

    this.getCustomerData();
    this.getBlockData();

    if (this.record) {
      this.changeBlock(this.record.BlockId);
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.editHistory = this.editHistory;

    const resp = data.Id ? await this.apartmentRepository.update(data) : await this.apartmentRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  changeRentBlockDetail(invalidRentBlockDetail: boolean) {
    this.invalidRentBlockDetail = invalidRentBlockDetail;
    this.validateForm.get('apartmentDetails')?.setValue(this.rentBlockDetailComponent.getValue());

    if (!this.invalidRentBlockDetail) {
      let apartmentDetails = this.validateForm.value.apartmentDetails;
      this.validateForm.get('UseAreaValue')?.setValue(
        apartmentDetails.reduce((x: number, curItem: any) => {
          return x + (curItem.TotalAreaDetailFloor ?? 0);
        }, 0)
      );
    }
  }

  async getCustomerData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,FullName';

    const resp = await this.customerRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.customers = resp.data;
    }
  }

  async changeBlock(event: any) {
    this.block = undefined;
    if (event) {
      const resp = await this.blockRepository.getById(event);

      if (resp.meta?.error_code == 200) {
        this.block = resp.data;
      }
    }
  }

  async getBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `TypeBlockEntity=${TypeBlockEntityEnum.BLOCK_RENT} AND TypeReportApply!=${TypeReportApplyEnum.NHA_RIENG_LE}`;
    paging.select = 'Id,Address';

    const resp = await this.blockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.blocks = resp.data;
    }
  }

  selectDay() {
    const selectedDate = this.validateForm.value.DateRecord;
  }
}
