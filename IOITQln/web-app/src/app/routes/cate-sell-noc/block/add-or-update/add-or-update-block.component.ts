import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { PositionCoefficientRepository } from 'src/app/infrastructure/repositories/position-coefficient.repository';
import { LandPriceRepository } from 'src/app/infrastructure/repositories/land-price.repository';
import { MainTextureRateTblComponent } from '../maintexture-rate-tbl/maintexture-rate-tbl.component';
import { BlockDetailComponent } from '../block-detail/block-detail.component';
import { DecreeEnum, TypeBlockEntityEnum, TypeReportApplyEnum, TypeApartmentDetailEnum, LandPriceType, AccessKey } from 'src/app/shared/utils/enums';
import { IsAlley_61_Coefficient } from 'src/app/shared/utils/consts';
import { No2LandPriceRepository } from 'src/app/infrastructure/repositories/no2-land-price.repository';
import { ApartmentDetailComponent } from 'src/app/routes/cate-sell-noc/apartment/apartment-detail/apartment-detail.component';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-block',
    templateUrl: './add-or-update-block.component.html'
})

export class AddOrUpdateBlockComponent implements OnInit {
    @ViewChild('mainTextureRateTblComponent') mainTextureRateTblComponent!: MainTextureRateTblComponent;
    @ViewChild('blockDetailComponent') blockDetailComponent!: BlockDetailComponent;
    @ViewChild('apartmentDetailComponent') apartmentDetailComponent!: ApartmentDetailComponent;

    @Input() record: NzSafeAny;
    @Input() block_rent: NzSafeAny;
    @Input() typeReportApply: NzSafeAny;
    @Input() code: NzSafeAny;
    @Input() typehouse_data: NzSafeAny;
    @Input() editHistory: NzSafeAny;
    @Input() isViewRecord?: boolean;

    validateForm!: FormGroup;
    loading: boolean = false;
    invalidApartmentDetail: boolean = false;

    floor_data: any[] = [];
    lane_data: any[] = [];
    landprice_data: any[] = [];             //bảng giá đất
    position_coefficient_data: any[] = [];   //danh sách hệ số vị trí tính giá đất
    levelblocks: any[] = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.BLOCK_MANAGEMENT);
    TypeReportApplyEnum = TypeReportApplyEnum;
    typeApartmentDetailEnum = TypeApartmentDetailEnum;
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private landPriceRepository: LandPriceRepository, private no2LandPriceRepository: No2LandPriceRepository,
        private blockRepository: BlockRepository, private positionCoefficientRepository: PositionCoefficientRepository,private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            TypeReportApply: [this.block_rent ? this.block_rent.TypeReportApply.toString() : (this.record ? this.record.TypeReportApply.toString() : this.typeReportApply), [Validators.required]],
            TypeBlockId: [this.block_rent ? this.block_rent.TypeBlockId : (this.record ? this.record.TypeBlockId : undefined), [Validators.required]],
            FloorApplyPriceChange: [this.record ? this.record.FloorApplyPriceChange : undefined, []],
            FloorBlockMap: [this.record ? this.record.FloorBlockMap : undefined, [Validators.required]],
            LandNo: [this.record ? this.record.LandNo : undefined, [Validators.required]],
            MapNo: [this.record ? this.record.MapNo : undefined, [Validators.required]],
            Width: [this.record ? this.record.Width : undefined, []],
            Deep: [this.record ? this.record.Deep : undefined, []],
            Code: [this.block_rent ? this.block_rent.Code : (this.record ? this.record.Code : this.code), []],
            // Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Address: [this.block_rent ? this.block_rent.Address : (this.record ? this.record.Address : undefined), [Validators.required]],
            Lane: [this.block_rent ? this.block_rent.Lane : (this.record ? this.record.Lane : undefined), [Validators.required]],
            Ward: [this.block_rent ? this.block_rent.Ward : (this.record ? this.record.Ward : undefined), [Validators.required]],
            District: [this.block_rent ? this.block_rent.District : (this.record ? this.record.District : undefined), [Validators.required]],
            Province: [this.record ? this.record.Province : undefined, [Validators.required]],
            Pdw: [this.block_rent ? [this.block_rent.Province, this.block_rent.District, this.block_rent.Ward] : (this.record ? [this.record.Province, this.record.District, this.record.Ward] : []), [Validators.required]],
            TypePile: [this.record ? this.record.TypePile.toString() : "1", [Validators.required]],
            ConstructionAreaNote: [this.record ? this.record.ConstructionAreaNote : undefined, []],
            ConstructionAreaValue: [this.block_rent ? this.block_rent.ConstructionAreaValue : (this.record ? this.record.ConstructionAreaValue : undefined), [Validators.required]],
            ConstructionAreaValue1: [this.record ? this.record.ConstructionAreaValue1 : undefined, []],
            ConstructionAreaValue2: [this.record ? this.record.ConstructionAreaValue2 : undefined, []],
            ConstructionAreaValue3: [this.record ? this.record.ConstructionAreaValue3 : undefined, []],
            UseAreaNote: [this.block_rent ? this.block_rent.UseAreaNote : (this.record ? this.record.UseAreaNote : undefined), []],
            UseAreaValue: [this.block_rent ? this.block_rent.UseAreaValue : (this.record ? this.record.UseAreaValue : undefined), [Validators.required]],
            UseAreaValue1: [this.record ? this.record.UseAreaValue1 : undefined, []],
            UseAreaValue2: [this.record ? this.record.UseAreaValue2 : undefined, []],
            levelBlockMaps: [this.block_rent ? (this.block_rent.levelBlockMaps.length == 0 ? undefined : this.block_rent.levelBlockMaps) : (this.record ? (this.record.levelBlockMaps.length == 0 ? [] : this.record.levelBlockMaps) : []), [Validators.required]],
            decreeMaps: [this.record ? this.record.decreeMaps : [], [Validators.required]],
            // blockDetails: [this.record ? this.record.blockDetails.map((item: any, index: number) => {
            //     item.index = index + 1;
            //     return item;
            // }) : [], []],
            blockDetails: [this.record ? this.record.blockDetails : [], []],
            blockMaintextureRaties: [this.record ? this.record.blockMaintextureRaties : [], []],
            // DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            SpecialCase: [this.record ? this.record.SpecialCase : undefined, []],
            LandUsePlanningInfo: [this.record ? this.record.LandUsePlanningInfo : undefined, []],
            HighwayPlanningInfo: [this.record ? this.record.HighwayPlanningInfo : undefined, []],
            LandAcquisitionSituationInfo: [this.record ? this.record.LandAcquisitionSituationInfo : undefined, []],
            LandPriceItemId_99: [this.record ? this.record.LandPriceItemId_99 : undefined, []],
            LandPriceItemValue_99: [this.record ? this.record.LandPriceItemValue_99 : undefined, []],
            LandPriceItemId_34: [this.record ? this.record.LandPriceItemId_34 : undefined, []],
            LandPriceItemValue_34: [this.record ? this.record.LandPriceItemValue_34 : undefined, []],
            LandPriceItemId_61: [this.record ? this.record.LandPriceItemId_61 : undefined, []],
            LandPriceItemValue_61: [this.record ? this.record.LandPriceItemValue_61 : undefined, []],
            PositionCoefficientId_99: [this.record ? this.record.PositionCoefficientId_99 : undefined, []],
            PositionCoefficientStr_99: [this.record ? this.record.PositionCoefficientStr_99 : undefined, []],
            LandscapeLocation_99: [this.record ? (this.record.LandscapeLocation_99 ? this.record.LandscapeLocation_99.toString() : undefined) : undefined, []],
            LandPriceRefinement_99: [this.record ? this.record.LandPriceRefinement_99 : undefined, []],
            LandScapePrice_99: [this.record ? this.record.LandScapePrice_99 : undefined, []],
            PositionCoefficientId_34: [this.record ? this.record.PositionCoefficientId_34 : undefined, []],
            PositionCoefficientStr_34: [this.record ? this.record.PositionCoefficientStr_34 : undefined, []],
            LandscapeLocation_34: [this.record ? (this.record.LandscapeLocation_34 ? this.record.LandscapeLocation_34.toString() : undefined) : undefined, []],
            LandPriceRefinement_34: [this.record ? this.record.LandPriceRefinement_34 : undefined, []],
            LandScapePrice_34: [this.record ? this.record.LandScapePrice_34 : undefined, []],
            PositionCoefficientId_61: [this.record ? this.record.PositionCoefficientId_61 : undefined, []],
            PositionCoefficientStr_61: [this.record ? this.record.PositionCoefficientStr_61 : undefined, []],
            LandscapeLocation_61: [this.record ? (this.record.LandscapeLocation_61 ? this.record.LandscapeLocation_61.toString() : undefined) : undefined, []],
            LandPriceRefinement_61: [this.record ? this.record.LandPriceRefinement_61 : undefined, []],
            LandScapePrice_61: [this.record ? this.record.LandScapePrice_61 : undefined, []],
            LevelAlley_34: [this.record ? (this.record.LevelAlley_34 ? this.record.LevelAlley_34.toString() : undefined) : undefined, []],
            LandscapeLocationInAlley_34: [this.record ? (this.record.LandscapeLocationInAlley_34 ? this.record.LandscapeLocationInAlley_34.toString() : undefined) : undefined, []],
            IsAlley_34: [this.record ? this.record.IsAlley_34 : undefined, []],
            AlleyPositionCoefficientId_34: [this.record ? this.record.AlleyPositionCoefficientId_34 : undefined, []],
            AlleyPositionCoefficientStr_34: [this.record ? this.record.AlleyPositionCoefficientStr_34 : undefined, []],
            AlleyLandScapePrice_34: [this.record ? this.record.AlleyLandScapePrice_34 : undefined, []],
            TextBasedInfo: [this.record ? this.record.TextBasedInfo : undefined, []],
            CaseApply_34: [this.record ? (this.record.CaseApply_34 ? this.record.CaseApply_34.toString() : undefined) : "2", []],
            LandSpecial: [this.record ? this.record.LandSpecial : undefined, []],
            LandAreaSpecial: [this.record ? this.record.LandAreaSpecial : undefined, []],
            LandAreaSpecialS1: [this.record ? this.record.LandAreaSpecialS1 : undefined, []],
            LandAreaSpecialS2: [this.record ? this.record.LandAreaSpecialS2 : undefined, []],
            LandAreaSpecialS3: [this.record ? this.record.LandAreaSpecialS3 : undefined, []],
            WidthSpecial: [this.record ? this.record.WidthSpecial : undefined, []],
            WidthSpecialS1: [this.record ? this.record.WidthSpecialS1 : undefined, []],
            WidthSpecialS2: [this.record ? this.record.WidthSpecialS2 : undefined, []],
            WidthSpecialS3: [this.record ? this.record.WidthSpecialS3 : undefined, []],
            LandPositionSpecial: [this.record ? this.record.LandPositionSpecial : undefined, []],
            TypeBlockEntity: [this.record ? this.record.TypeBlockEntity : TypeBlockEntityEnum.BLOCK_NORMAL, []],
            IsFrontOfLine_61: [this.record ? this.record.IsFrontOfLine_61 : true, []],
            TypeAlley_61: [this.record ? (this.record.TypeAlley_61 ? this.record.TypeAlley_61.toString() : undefined) : undefined, []],
            LandscapeLocationInAlley_61: [this.record ? (this.record.LandscapeLocationInAlley_61 ? this.record.LandscapeLocationInAlley_61.toString() : undefined) : undefined, []],
            IsAlley_61: [this.record ? this.record.IsAlley_61 : undefined, []],
            No2LandScapePrice_61: [this.record ? this.record.No2LandScapePrice_61 : undefined, []],
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
            LandscapeAreaValue: [this.record ? this.record.LandscapeAreaValue : undefined, []],
            LandscapePrivateAreaValue: [this.record ? this.record.LandscapePrivateAreaValue : undefined, []],
            ApprovedForConstructionOnTheApartmentYard: [this.record ? this.record.ApprovedForConstructionOnTheApartmentYard : undefined, []],
            ParentId: [this.record ? this.record.ParentId : undefined, []],
            ParentTypeReportApply: [this.record ? this.record.ParentTypeReportApply : undefined, []],
            SellConstructionAreaValue: [this.block_rent ? this.block_rent.SellConstructionAreaValue : (this.record ? this.record.SellConstructionAreaValue : undefined), []],
            SellConstructionAreaNote: [this.block_rent ? this.block_rent.SellConstructionAreaNote : (this.record ? this.record.SellConstructionAreaNote : undefined), []],
            SellLandArea: [this.record ? this.record.SellLandArea : undefined, []],
            pricingApartmentLandDetails: [this.record ? this.record.pricingApartmentLandDetails : undefined, []],
            Attactment: [this.record ? this.record.Attactment : undefined, []],
            UserIdCreateAttactment: [this.record ? this.record.UserIdCreateAttactment : undefined, []],
            ExceedingLimitDeep: [this.record ? this.record.ExceedingLimitDeep : undefined, []],
        });

        if (this.record) {
            this.changeDecreeType1(this.record.decreeMaps, true);
        }

        if (this.block_rent) {
            this.levelblocks = this.block_rent.levelBlockMaps;
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

        if (data.decreeMaps) {
            data.decreeMaps.forEach((x: any) => {
                x.DecreeType1Id = x.DecreeType1Id ?? x.key;

                return x;
            });
        }

        if (data.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
            data.blockDetails = this.blockDetailComponent.getValue();
        }

        data.blockMaintextureRaties = this.mainTextureRateTblComponent.getValue();
        data.editHistory = this.editHistory;

        const resp = data.Id ? await this.blockRepository.update(data) : await this.blockRepository.addNew(data);
        if (resp.meta?.error_code == 200) {
            this.loading = false;
            this.drawerRef.close(data);
        }
        else {
            this.loading = false;
        }
    }

    close(): void {
        this.drawerRef.close();
    }

    changeLevelBlock(evt: any) {
        if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            let data = this.validateForm.value.apartmentDetails;
            data.forEach((item: any) => {
                let levelblock = evt.find((x: any) => x.LevelId == item.Level || x.key == item.Level);
                if (item.Level && !levelblock) {
                    evt.push({
                        LevelId: item.Level
                    });
                }
            });

            this.levelblocks = [...evt];
        }
        else {
            this.levelblocks = evt;
        }
    }

    changeDecreeType1(event: any, init: boolean) {
        if (event) {
            this.getPositionCoefficient(event);
        }
        else {
            this.position_coefficient_data = [];
        }

        this.getLandPrice(init);

    }

    //lấy ds hệ số vị trí tính giá đất theo nghị định
    async getPositionCoefficient(decreeMaps: any) {
        this.position_coefficient_data = [];

        if (decreeMaps.length) {
            let paging: GetByPageModel = new GetByPageModel();
            paging.page_size = 0;
            paging.query = decreeMaps.map((d: any) => "DecreeType1Id=" + (d.key ?? d.DecreeType1Id)).join(' OR ');

            const resp = await this.positionCoefficientRepository.getByPage(paging);

            if (resp.meta?.error_code == 200) {
                this.position_coefficient_data = resp.data;
            }
        }
    }

    changePositionCoefficientId(decree: DecreeEnum) {
        let landscapeLocation;
        let positionCoefficientId: number = 0;

        switch (decree) {
            case DecreeEnum.ND_99:
                landscapeLocation = this.validateForm.value.LandscapeLocation_99;
                positionCoefficientId = this.validateForm.value.PositionCoefficientId_99;
                this.validateForm.get('PositionCoefficientStr_99')?.setValue(this.calcLandscapeLocation(landscapeLocation, positionCoefficientId, decree));
                break;
            case DecreeEnum.ND_34:
                landscapeLocation = this.validateForm.value.LandscapeLocation_34;
                positionCoefficientId = this.validateForm.value.PositionCoefficientId_34;
                this.validateForm.get('PositionCoefficientStr_34')?.setValue(this.calcLandscapeLocation(landscapeLocation, positionCoefficientId, decree));
                break;
            case DecreeEnum.ND_61:
                // landscapeLocation = this.validateForm.value.LandscapeLocation_61;
                // positionCoefficientId = this.validateForm.value.PositionCoefficientId_61;
                // this.validateForm.get('PositionCoefficientStr_61')?.setValue(this.calcLandscapeLocation(landscapeLocation, positionCoefficientId, decree));
                let landPriceItemValue_61 = this.validateForm.value.LandPriceItemValue_61;
                let isFrontOfLine_61 = this.validateForm.value.IsFrontOfLine_61;

                if (isFrontOfLine_61) {
                    this.validateForm.get('LandScapePrice_61')?.setValue(landPriceItemValue_61);
                    this.validateForm.get('No2LandScapePrice_61')?.setValue(undefined);
                }
                else {
                    let typeAlley_61 = this.validateForm.value.TypeAlley_61;
                    let width = this.validateForm.value.Width ?? 0;
                    let isAlley_61 = this.validateForm.value.IsAlley_61;

                    if (landPriceItemValue_61 && typeAlley_61 && width != undefined) {
                        this.getLandScapePrice61(landPriceItemValue_61, typeAlley_61, width, isAlley_61);
                    }
                    else {
                        this.validateForm.get('LandScapePrice_61')?.setValue(undefined);
                        this.validateForm.get('No2LandScapePrice_61')?.setValue(undefined);
                    }
                }
                break;
            default:
                landscapeLocation = this.validateForm.value.LandscapeLocationInAlley_34;
                positionCoefficientId = this.validateForm.value.AlleyPositionCoefficientId_34;
                let levelAlley_34 = this.validateForm.value.LevelAlley_34;
                let isAlley_34 = this.validateForm.value.IsAlley_34;
                this.validateForm.get('AlleyPositionCoefficientStr_34')?.setValue(this.calcLandscapeLocation(landscapeLocation, positionCoefficientId, decree, levelAlley_34, isAlley_34));
                break;
        }
    }

    calcLandscapeLocation(landscapeLocation?: number, positionCoefficientId?: number, decree?: DecreeEnum, levelAlley_34?: number, isAlley_34?: boolean) {
        let str = "";
        let value = undefined;

        if (landscapeLocation && positionCoefficientId) {
            let position_coefficient_data = this.position_coefficient_data.find(x => x.Id == positionCoefficientId);
            if (position_coefficient_data) {
                if (decree != DecreeEnum.SPECIAL) {
                    switch (true) {
                        case landscapeLocation == 1:
                            str = `${position_coefficient_data.LocationValue1}`;
                            value = position_coefficient_data.LocationValue1;
                            break;
                        case landscapeLocation == 2:
                            str = `${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
                            value = position_coefficient_data.LocationValue1 * position_coefficient_data.LocationValue2;
                            break;
                        case landscapeLocation == 3:
                            str = `${position_coefficient_data.LocationValue3} x ${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
                            value = position_coefficient_data.LocationValue1 * position_coefficient_data.LocationValue2 * position_coefficient_data.LocationValue3;
                            break;
                        case landscapeLocation == 4:
                            str = `${position_coefficient_data.LocationValue4} x ${position_coefficient_data.LocationValue3} x ${position_coefficient_data.LocationValue2} x ${position_coefficient_data.LocationValue1}`;
                            value = position_coefficient_data.LocationValue1 * position_coefficient_data.LocationValue2 * position_coefficient_data.LocationValue3 * position_coefficient_data.LocationValue4;
                            break;
                        default: break;
                    }
                }
                else {
                    console.log("landscapeLocation: " + landscapeLocation);
                    console.log("levelAlley_34: " + levelAlley_34);

                    switch (true) {
                        case landscapeLocation == 1:
                            str = `${position_coefficient_data.AlleyValue1}`;
                            value = position_coefficient_data.AlleyValue1;
                            break;
                        case landscapeLocation == 2:
                            str = `${position_coefficient_data.AlleyValue2} x ${position_coefficient_data.AlleyValue1}`;
                            value = position_coefficient_data.AlleyValue1 * position_coefficient_data.AlleyValue2;
                            break;
                        case landscapeLocation == 3:
                            str = `${position_coefficient_data.AlleyValue3} x ${position_coefficient_data.AlleyValue2} x ${position_coefficient_data.AlleyValue1}`;
                            value = position_coefficient_data.AlleyValue1 * position_coefficient_data.AlleyValue2 * position_coefficient_data.AlleyValue3;
                            break;
                        case landscapeLocation == 4:
                            str = `${position_coefficient_data.AlleyValue4} x ${position_coefficient_data.AlleyValue3} x ${position_coefficient_data.AlleyValue2} x ${position_coefficient_data.AlleyValue1}`;
                            value = position_coefficient_data.AlleyValue1 * position_coefficient_data.AlleyValue2 * position_coefficient_data.AlleyValue3 * position_coefficient_data.AlleyValue4;
                            break;
                        default: break;
                    }

                    switch (true) {
                        case levelAlley_34 == 1:
                            break;
                        case levelAlley_34 == 2:
                            str = `${position_coefficient_data.AlleyLevel2} x ${str}`;
                            value = value * position_coefficient_data.AlleyLevel2;
                            break;
                        case levelAlley_34 == 3:
                            str = `${position_coefficient_data.AlleyOther} x ${position_coefficient_data.AlleyLevel2} x ${str}`;
                            value = value * position_coefficient_data.AlleyOther * position_coefficient_data.AlleyLevel2;
                            break;
                        default: break;
                    }

                    if (isAlley_34) {
                        str = `${position_coefficient_data.AlleyLand} x ${str}`;
                        value = value * position_coefficient_data.AlleyLand;
                    }
                }
            }
        }

        // let deep = this.validateForm.value.Deep;
        let exceedingLimitDeep = this.validateForm.value.ExceedingLimitDeep;
        let width = this.validateForm.value.Width;
        let typeReportApply = this.validateForm.value.TypeReportApply;

        switch (decree) {
            case DecreeEnum.ND_99:
                let landPriceItemValue_99 = this.validateForm.value.LandPriceItemValue_99;
                let landPriceRefinement_99 = this.validateForm.value.LandPriceRefinement_99;

                if (value && landPriceItemValue_99) {
                    let landScapePrice_99 = landPriceItemValue_99 * value * (landPriceRefinement_99 && exceedingLimitDeep ? ((100 - landPriceRefinement_99) / 100) : 1);
                    let landscapeLocation_99 = this.validateForm.value.LandscapeLocation_99;

                    if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE && (landscapeLocation_99 == 1 || landscapeLocation_99 == "1"))
                        landScapePrice_99 = landScapePrice_99 * (width < 3 ? 1 : 1.2);

                    this.validateForm.get('LandScapePrice_99')?.setValue(Math.round(landScapePrice_99));
                }
                else {
                    this.validateForm.get('LandScapePrice_99')?.setValue(undefined);
                }
                break;
            case DecreeEnum.ND_34:
                let landPriceItemValue_34 = this.validateForm.value.LandPriceItemValue_34;
                let landPriceRefinement_34 = this.validateForm.value.LandPriceRefinement_34;

                if (value && landPriceItemValue_34) {
                    let landScapePrice_34 = landPriceItemValue_34 * value * (landPriceRefinement_34 && exceedingLimitDeep ? ((100 - landPriceRefinement_34) / 100) : 1);
                    this.validateForm.get('LandScapePrice_34')?.setValue(Math.round(landScapePrice_34));
                }
                else {
                    this.validateForm.get('LandScapePrice_34')?.setValue(undefined);
                }
                break;
            case DecreeEnum.ND_61:
                let landPriceItemValue_61 = this.validateForm.value.LandPriceItemValue_61;
                let landPriceRefinement_61 = this.validateForm.value.LandPriceRefinement_61;

                if (value && landPriceItemValue_99) {
                    let landScapePrice_61 = Math.round(landPriceItemValue_61 * value * (landPriceRefinement_61 && exceedingLimitDeep ? ((100 - landPriceRefinement_61) / 100) : 1));
                    this.validateForm.get('LandScapePrice_61')?.setValue(landScapePrice_61);
                }
                else {
                    this.validateForm.get('LandScapePrice_61')?.setValue(undefined);
                }
                break;
            default:
                let alleyLandPriceItemValue_34 = this.validateForm.value.LandPriceItemValue_34;
                // let alleyLandPriceRefinement_34 = this.validateForm.value.LandPriceRefinement_34;

                if (value && alleyLandPriceItemValue_34) {
                    let alleyLandScapePrice_34 = alleyLandPriceItemValue_34 * value;
                    this.validateForm.get('AlleyLandScapePrice_34')?.setValue(Math.round(alleyLandScapePrice_34));
                }
                else {
                    this.validateForm.get('AlleyLandScapePrice_34')?.setValue(undefined);
                }
                break;
        }

        return str;
    }

    async getLandPrice(init: boolean) {
        if (!init) {
            this.validateForm.get('LandPriceItemId')?.setValue(undefined);
            this.validateForm.get('LandPriceItemValue')?.setValue(undefined);
        }

        let decreeMaps = this.validateForm.value.decreeMaps;
        let districtId = this.validateForm.value.District;

        this.landprice_data = [];

        if (decreeMaps && districtId) {
            let list_decree = decreeMaps.reduce((res: any, cur: any) => {
                res.push(cur.key ?? cur.DecreeType1Id);
                return res;
            }, []);

            let laneId = this.validateForm.value.Lane;
            let lane = this.lane_data.find(x => x.Id == laneId);

            const resp = await this.landPriceRepository.getLandPriceItemsMultiDecreeType1Id(districtId, list_decree, LandPriceType.NOC, lane?.Name);

            if (resp.meta?.error_code == 200) {
                this.landprice_data = resp.data;

                // if (resp.metadata == 1 && !init) {
                //     this.validateForm.get('LandPriceItemId')?.setValue(this.landprice_data[0].Id);
                //     this.validateForm.get('LandPriceItemValue')?.setValue(this.landprice_data[0].Value);
                // }
            }
        }
    }

    changeLandPriceItem(decree: DecreeEnum) {
        this.changePositionCoefficientId(decree);
        if (decree == DecreeEnum.ND_34) this.changePositionCoefficientId(DecreeEnum.SPECIAL);
    }

    async getLandScapePrice61(landPriceItemValue_61: number, typeAlley_61: number, width: number, isAlley_61: boolean): Promise<number> {
        let landScapePrice61;
        const resp = await this.no2LandPriceRepository.getLandScapePrice61({
            LandPriceItemValue_61: landPriceItemValue_61,
            TypeAlley: typeAlley_61,
            Width: width
        });

        if (resp.meta?.error_code == 200) {
            landScapePrice61 = resp.data;

            this.validateForm.get('No2LandScapePrice_61')?.setValue(landScapePrice61);
            this.validateForm.get('LandScapePrice_61')?.setValue(landScapePrice61 * (!isAlley_61 ? 1 : IsAlley_61_Coefficient));
        }

        return landScapePrice61;
    }

    changeLevelBlockMaps(invalidApartmentDetail: boolean) {
        this.invalidApartmentDetail = invalidApartmentDetail;
        if (!invalidApartmentDetail) {
            this.validateForm.get('apartmentDetails')?.setValue(this.apartmentDetailComponent.getValue());

            let levelblocks = [...this.levelblocks];
            this.levelblocks = [];

            if (this.validateForm.value.TypeReportApply) {
                let data = this.validateForm.value.apartmentDetails;
                let apartmentLandDetails = this.validateForm.value.apartmentLandDetails ?? [];
                let apartmentLandDetailsResult: any[] = [];

                data.forEach((item: any) => {
                    // let levelblockExist = this.validateForm.value.levelBlockMaps.find((x: any) => x.LevelId == item.Level);
                    let levelblock = levelblocks.find((x: any) => x.LevelId == item.Level || x.key == item.Level);
                    if (item.Level && !levelblock) {
                        levelblocks.push({
                            LevelId: item.Level
                        });
                    }

                    //ApartmentLandDetail
                    if (item.DecreeType1Id && item.TermApply) {
                        let apartmentLandDetail = apartmentLandDetails.find((x: any) => x.DecreeType1Id == item.DecreeType1Id && x.TermApply == item.TermApply);
                        if (apartmentLandDetail) {
                            let apartmentLandDetailsResultItem = apartmentLandDetailsResult.find((x: any) => x.DecreeType1Id == item.DecreeType1Id && x.TermApply == item.TermApply);
                            if (!apartmentLandDetailsResultItem) {
                                apartmentLandDetailsResult.push(apartmentLandDetail);
                            }
                        }
                        else {
                            apartmentLandDetailsResult.push({
                                DecreeType1Id: item.DecreeType1Id,
                                TermApply: item.TermApply,
                                GeneralArea: 0,
                                PrivateArea: 0
                            });
                        }
                    }
                });

                this.validateForm.get('apartmentLandDetails')?.setValue(apartmentLandDetailsResult);
                this.levelblocks = [...levelblocks];
            }
        }
    }

    async chooseBlock() {
        await new Promise(f => setTimeout(f, 50));
        if (this.mainTextureRateTblComponent != undefined) this.mainTextureRateTblComponent.emitChooseBlock();
    }
}
