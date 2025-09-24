import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { RentBlockDetailComponent } from '../rent-block-detail/rent-block-detail.component';
import { TypeReportApplyEnum, TypeBlockEntityEnum, AccessKey } from 'src/app/shared/utils/enums';
import { LevelBlock, TypeReportApply, UsageStatus } from 'src/app/shared/utils/consts';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { TypeHouse } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-rent-block',
  templateUrl: './add-or-update-rent-block.component.html'
})
export class AddOrUpdateRentBlockComponent implements OnInit {
  // @ViewChild('mainTextureRateTblComponent') mainTextureRateTblComponent!: MainTextureRateTblComponent;
  @ViewChild('rentBlockDetailComponent') rentBlockDetailComponent!: RentBlockDetailComponent;
  @Input() record: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;
  @Input() editHistory: NzSafeAny;
  @Input() isViewRecord?: boolean;

  validateForm!: FormGroup;
  loading: boolean = false;
  invalidRentBlockDetail: boolean = false;

  typeReportApply_data = TypeReportApply;
  levelblockmap_data = LevelBlock;
  usageStatus_data = UsageStatus;
  TypeHouse = TypeHouse;
  typeReportApplyEnum = TypeReportApplyEnum;
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENT_BLOCK);
  lane_data: any[] = [];
  customers: any[] = [];
  floor: any[] = [];

  newDate = convertDate(new Date().toString());
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private customerRepository: CustomerRepository,
    private blockRepository: BlockRepository,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, [Validators.required]],
      TypeBlockId: [this.record ? this.record.TypeBlockId : undefined, [Validators.required]],
      Code: [this.record ? this.record.Code : undefined, []],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
      Lane: [this.record ? this.record.Lane : undefined, [Validators.required]],
      Ward: [this.record ? this.record.Ward : undefined, [Validators.required]],
      District: [this.record ? this.record.District : undefined, [Validators.required]],
      Province: [this.record ? this.record.Province : undefined, [Validators.required]],
      Pdw: [this.record ? [this.record.Province, this.record.District, this.record.Ward] : undefined, [Validators.required]],
      ConstructionAreaValue: [this.record ? this.record.ConstructionAreaValue : undefined, []],
      UseAreaValue: [this.record ? this.record.UseAreaValue : undefined, []],
      levelBlockMaps: [
        this.record ? (this.record.levelBlockMaps.length == 0 ? undefined : this.record.levelBlockMaps) : undefined,
        [Validators.required]
      ],
      blockDetails: [
        this.record
          ? this.record.blockDetails.map((item: any) => {
              item.Level = item.Level.toString();
              item.DisposeTime = item.DisposeTime ? convertDate(item.DisposeTime) : undefined;
              return item;
            })
          : [],
        []
      ],
      TypeBlockEntity: [this.record ? this.record.TypeBlockEntity : TypeBlockEntityEnum.BLOCK_RENT, []],
      CampusArea: [this.record ? this.record.CampusArea : undefined, []],
      EstablishStateOwnership: [this.record ? this.record.EstablishStateOwnership : undefined, []],
      Dispute: [this.record ? this.record.Dispute : undefined, []],
      Blueprint: [this.record ? this.record.Blueprint : undefined, []],
      CustomerId: [this.record ? this.record.CustomerId : undefined, []],
      UsageStatus: [this.record ? (this.record.UsageStatus ? this.record.UsageStatus.toString() : undefined) : undefined, []],
      UsageStatusNote: [this.record ? this.record.UsageStatusNote : undefined, []],
      UseAreaNote1: [this.record ? this.record.UseAreaNote1 : undefined, []],
      UseAreaNote: [this.record ? this.record.UseAreaNote : undefined, []],
      Floor: [this.record ? this.record.Floor : undefined],
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
    for (let i = 1; i <= 40; i++) {
      this.floor.push(i);
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    if (data.levelBlockMaps) {
      data.levelBlockMaps.forEach((x: any) => {
        x.LevelId = x.LevelId ?? x.key;

        return x;
      });
    }

    data.editHistory = this.editHistory;

    const resp = data.Id ? await this.blockRepository.update(data) : await this.blockRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  changeCodeValidation(typeReportApply: TypeReportApplyEnum) {
    if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) this.validateForm.get('Code')?.setValidators([Validators.required]);
    else this.validateForm.get('Code')?.setValidators(null);

    // this.validateForm.value.Code.setValidators(typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE ? null : [Validators.required]);
    this.cdr.detectChanges();
    this.validateForm.updateValueAndValidity();
  }

  close(): void {
    this.drawerRef.close();
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2;
  };

  changeRentBlockDetail(invalidRentBlockDetail: boolean) {
    this.invalidRentBlockDetail = invalidRentBlockDetail;
    this.validateForm.get('blockDetails')?.setValue(this.rentBlockDetailComponent.getValue());

    if (!this.invalidRentBlockDetail) {
      let blockDetails = this.validateForm.value.blockDetails;
      this.validateForm.get('UseAreaValue')?.setValue(
        blockDetails.reduce((x: number, curItem: any) => {
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

  selectDay() {
    const selectedDate = this.validateForm.value.DateRecord;
  }
}
