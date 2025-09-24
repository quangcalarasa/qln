import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeLandSpecial } from 'src/app/shared/utils/consts';
import { _ } from 'ajv';
import { MainTextureRateTblComponent } from 'src/app/routes/cate-sell-noc/block/maintexture-rate-tbl/maintexture-rate-tbl.component';
import { ApartmentDetailComponent } from 'src/app/routes/cate-sell-noc/apartment/apartment-detail/apartment-detail.component';
import { ApartmentLandDetailComponent } from 'src/app/routes/cate-sell-noc/apartment/apartment-land-detail/apartment-land-detail.component';
import { TypeApartmentEntityEnum, TypeApartmentDetailEnum, TypeApartmentLandDetailEnum, TypeReportApplyEnum, AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-add-or-update-apartment',
  templateUrl: './add-or-update-apartment.component.html'
})
export class AddOrUpdateApartmentComponent implements OnInit {
  @ViewChild('mainTextureRateTblComponent') mainTextureRateTblComponent!: MainTextureRateTblComponent;
  @ViewChild('apartmentDetailComponent') apartmentDetailComponent!: ApartmentDetailComponent;
  @ViewChild('apartmentLandDetailComponent') apartmentLandDetailComponent!: ApartmentLandDetailComponent;

  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() apartment_rent: NzSafeAny;
  @Input() code: NzSafeAny;
  @Input() editHistory: NzSafeAny;
  @Input() isViewRecord?: boolean;

  role = this.commonService.CheckAccessKeyRole(AccessKey.APARTMENT_MANAGEMENT);
  block: any;

  block_data: any[] = [];
  landpositionspeical_data = TypeLandSpecial; //ds thửa đất có hình dạng đặc biệt
  typeApartmentDetailEnum = TypeApartmentDetailEnum;
  typeApartmentLandDetailEnum = TypeApartmentLandDetailEnum;
  TypeReportApplyEnum = TypeReportApplyEnum;

  levelblocks: any[] = [];

  invalidApartmentDetail: boolean = false;
  invalidApartmentLandDetail: boolean = false;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private apartmentRepository: ApartmentRepository,
    private commonService: CommonService,

  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, [Validators.required]],
      ParentTypeReportApply: [this.record ? (this.record.ParentTypeReportApply ? this.record.ParentTypeReportApply.toString() : undefined) : undefined, []],
      BlockId: [this.record ? this.record.BlockId : undefined, [Validators.required]],
      Code: [this.apartment_rent ? this.apartment_rent.Code : (this.record ? this.record.Code : this.code), [Validators.required]],
      Address: [this.apartment_rent ? this.apartment_rent.Address : (this.record ? this.record.Address : undefined), [Validators.required]],
      ConstructionAreaValue: [this.record ? this.record.ConstructionAreaValue : undefined, [Validators.required]],
      ConstructionAreaValue1: [this.record ? this.record.ConstructionAreaValue1 : undefined, []],
      ConstructionAreaValue2: [this.record ? this.record.ConstructionAreaValue2 : undefined, []],
      ConstructionAreaValue3: [this.record ? this.record.ConstructionAreaValue3 : undefined, []],
      UseAreaValue: [this.apartment_rent ? this.apartment_rent.UseAreaValue : (this.record ? this.record.UseAreaValue : undefined), [Validators.required]],
      UseAreaValue1: [this.record ? this.record.UseAreaValue1 : undefined, []],
      UseAreaValue2: [this.record ? this.record.UseAreaValue2 : undefined, []],
      LandscapeAreaValue: [this.record ? this.record.LandscapeAreaValue : undefined, [Validators.required]],
      LandscapeAreaValue1: [this.record ? this.record.LandscapeAreaValue1 : undefined, []],
      LandscapeAreaValue2: [this.record ? this.record.LandscapeAreaValue2 : undefined, []],
      LandscapeAreaValue3: [this.record ? this.record.LandscapeAreaValue3 : undefined, []],
      apartmentDetails: [
        this.record
          ? this.record.apartmentDetails.map((item: any, index: number) => {
            item.index = index + 1;
            item.Level = item.Level ? item.Level.toString() : undefined;
            item.TermApply = item.TermApply ? item.TermApply.toString() : undefined;
            return item;
          })
          : [],
        []
      ],
      apartmentLandDetails: [
        this.record
          ? this.record.apartmentLandDetails.map((item: any, index: number) => {
            item.index = index + 1;
            item.TermApply = item.TermApply ? item.TermApply.toString() : undefined;
            return item;
          })
          : [],
        []
      ],
      InLimit40Percent: [this.record ? this.record.InLimit40Percent : undefined, []],
      OutLimit100Percent: [this.record ? this.record.OutLimit100Percent : undefined, []],
      blockMaintextureRaties: [this.record ? this.record.blockMaintextureRaties : [], []],
      TypeApartmentEntity: [this.record ? this.record.TypeApartmentEntity : TypeApartmentEntityEnum.APARTMENT_NORMAL, []],
      ApprovedForConstructionOnTheApartmentYard: [this.record ? this.record.ApprovedForConstructionOnTheApartmentYard : undefined, []],
      ApprovedForConstructionOnTheApartmentYardLandscape: [this.record ? this.record.ApprovedForConstructionOnTheApartmentYardLandscape : undefined, []],
      ConstructionAreaNote: [this.record ? this.record.ConstructionAreaNote : undefined, []],
      UseAreaNote: [this.record ? this.record.ConstructionAreaNote : undefined, []],
      SellLandArea: [this.record ? this.record.SellLandArea : undefined, []],
      pricingApartmentLandDetails: [this.record ? this.record.pricingApartmentLandDetails : undefined, []]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    data.blockMaintextureRaties = this.mainTextureRateTblComponent.getValue();
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

  changeLevelBlockMaps(invalidApartmentDetail: boolean) {
    this.invalidApartmentDetail = invalidApartmentDetail;
    this.validateForm.get('apartmentDetails')?.setValue(this.apartmentDetailComponent.getValue());

    if (this.block) {
      let levelblocks: any[] = [];
      this.levelblocks = [];

      let data = this.validateForm.value.apartmentDetails;
      let apartmentLandDetails = this.validateForm.value.apartmentLandDetails ?? [];
      let apartmentLandDetailsResult: any[] = [];

      data.forEach((item: any) => {
        let levelblockExist = this.block.levelBlockMaps.find((x: any) => x.LevelId == item.Level);
        let levelblock = levelblocks.find((x: any) => x.LevelId == item.Level);
        if (!levelblock && item.Level && !levelblockExist) {
          levelblocks.push({
            LevelId: item.Level
          });
        }

        //ApartmentLandDetail
        if (item.DecreeType1Id && item.TermApply) {
          let apartmentLandDetail = apartmentLandDetails.find((x: any) => x.DecreeType1Id == item.DecreeType1Id && x.TermApply == item.TermApply);
          if (apartmentLandDetail && this.validateForm.value.TypeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU) {
            let apartmentLandDetailsResultItem = apartmentLandDetailsResult.find((x: any) => x.DecreeType1Id == item.DecreeType1Id && x.TermApply == item.TermApply);
            if (!apartmentLandDetailsResultItem) {
              apartmentLandDetailsResult.push(apartmentLandDetail);
            }
          }
          else {
            if (this.validateForm.value.TypeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU) {
              apartmentLandDetailsResult.push({
                DecreeType1Id: item.DecreeType1Id,
                TermApply: item.TermApply,
                GeneralArea: 0,
                PrivateArea: 0
              });
            }
            else {
              //Kiểm tra xem cấp nhà có thuộc cấp nhà của chung cư không
              let checkLevelBlockMap = this.block?.levelBlockMaps.find((x: any) => x.LevelId == item.Level);
              apartmentLandDetailsResult.push({
                DecreeType1Id: item.DecreeType1Id,
                TermApply: item.TermApply,
                Level: item.Level,
                FloorId: item.FloorId,
                AreaId: item.AreaId,
                PrivateArea: checkLevelBlockMap ? item.PrivateArea : 0,
                FloorApplyPriceChange: item.FloorApplyPriceChange,
                CoefficientDistribution: item.CoefficientDistribution
              });
            }
          }
        }
      });

      this.validateForm.get('apartmentLandDetails')?.setValue(apartmentLandDetailsResult);
      this.levelblocks = [...levelblocks];
    }

    let typeReportApply = this.validateForm.value.TypeReportApply;
    if (typeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU) {
      let apartmentLandDetails = this.validateForm.value.apartmentLandDetails;
      this.validateForm.get('LandscapeAreaValue2')?.setValue(apartmentLandDetails.reduce((x: number, curItem: any) => { return x + curItem.PrivateArea }, 0));
      this.validateForm.get('LandscapeAreaValue3')?.setValue(apartmentLandDetails.reduce((x: number, curItem: any) => { return x + (curItem.GeneralArea ?? 0) }, 0));

      this.calcLandscapeAreaValue();
    }
  }

  calcLandscapeAreaValue() {
    let landscapeAreaValue1 = this.validateForm.value.LandscapeAreaValue1;
    let landscapeAreaValue2 = this.validateForm.value.LandscapeAreaValue2;
    let landscapeAreaValue3 = this.validateForm.value.LandscapeAreaValue3;

    if (!landscapeAreaValue1 && !landscapeAreaValue2 && !landscapeAreaValue3) {
      this.validateForm.get('LandscapeAreaValue')?.setValue(undefined);
    } else {
      let landscapeAreaValue = (landscapeAreaValue1 ?? 0) + (landscapeAreaValue2 ?? 0) + (landscapeAreaValue3 ?? 0);
      this.validateForm.get('LandscapeAreaValue')?.setValue(landscapeAreaValue);
    }
  }

  changeApartmentLandDetail(apartmentLandDetails: any) {
    this.invalidApartmentLandDetail = false;

    this.validateForm.get('apartmentLandDetails')?.setValue(apartmentLandDetails);

    let typeReportApply = this.validateForm.value.TypeReportApply;
    if (typeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU) {
      this.validateForm.get('LandscapeAreaValue2')?.setValue(apartmentLandDetails.reduce((x: number, curItem: any) => { return x + curItem.PrivateArea }, 0));
      this.validateForm.get('LandscapeAreaValue3')?.setValue(apartmentLandDetails.reduce((x: number, curItem: any) => { return x + (curItem.GeneralArea ?? 0) }, 0));

      this.calcLandscapeAreaValue();
    }
  }
}
