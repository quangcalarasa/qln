import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup, Validators } from '@angular/forms';
import { Decree, TypeReportApply } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum, DecreeEnum, TypeBlockEntityEnum } from 'src/app/shared/utils/enums';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';

@Component({
    selector: 'app-block-apply-range',
    templateUrl: './apply-range.component.html'
})

export class ApplyRangeComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();
    @Output() eventEmitterChooseBlock: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;

    block_data: any[] = [];

    decree_type1_data = Decree;
    typeReportApply_data = TypeReportApply;

    TypeReportApplyEnum = TypeReportApplyEnum;
    DecreeEnum = DecreeEnum;
    constructor(
        private cdr: ChangeDetectorRef,
        private blockRepository: BlockRepository
    ) { }

    ngOnInit(): void {
        this.changeFormValidation(this.validateForm.value.TypeReportApply);

        if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.validateForm.value.Id) {
            let decreeMaps = this.validateForm.value.decreeMaps;
            this.getBlockByDecree(decreeMaps);
        }
    }

    compareFnDecree = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.key === o2.key || o1.DecreeType1Id === parseInt(o2.key) : o1 === o2);
    };

    changeDecreeType1(event: any) {
        this.eventEmitter.emit(event);
    }

    changeFormValidation(typeReportApply: TypeReportApplyEnum) {
        if (typeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
            this.validateForm.get('Code')?.setValidators(null);
            this.validateForm.get('FloorApplyPriceChange')?.setValidators([Validators.required]);
            this.validateForm.get('LandscapeAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ParentId')?.setValidators(null);
        }
        else if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            this.validateForm.get('Code')?.setValidators([Validators.required]);
            this.validateForm.get('FloorApplyPriceChange')?.setValidators(null);
            this.validateForm.get('LandscapeAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('UseAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ParentId')?.setValidators(null);
        }
        else if (typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
            this.validateForm.get('Code')?.setValidators(null);
            this.validateForm.get('FloorApplyPriceChange')?.setValidators([Validators.required]);
            this.validateForm.get('LandscapeAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators(null);
            this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ParentId')?.setValidators(null);
        }
        else if (typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) {
            this.validateForm.get('Code')?.setValidators(null);
            this.validateForm.get('FloorApplyPriceChange')?.setValidators(null);
            this.validateForm.get('LandscapeAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators(null);
            this.validateForm.get('ParentId')?.setValidators([Validators.required]);

            if (this.validateForm.value.ParentTypeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU) {
                this.validateForm.get('ConstructionAreaValue')?.setValidators(null);
            }
            else {
                this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            }
        }
        else {
            this.validateForm.get('Code')?.setValidators(null);
            this.validateForm.get('FloorApplyPriceChange')?.setValidators(null);
            this.validateForm.get('LandscapeAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('ParentId')?.setValidators(null);
        }

        this.cdr.detectChanges();
        this.validateForm.updateValueAndValidity();
    }

    async getBlockByDecree(formValueDecreeMaps: any) {
        this.block_data = [];

        if (formValueDecreeMaps) {
            if (formValueDecreeMaps.length) {
                let decreeMaps = formValueDecreeMaps.reduce((res: any, cur: any) => {
                    res.push(parseInt(cur.key ?? cur.DecreeType1Id));
                    return res;
                }, []);

                const resp = await this.blockRepository.getBlockByDecree(decreeMaps);

                if (resp.meta?.error_code == 200) {
                    this.block_data = resp.data;
                }
            }
        }
    }

    changeDecree() {
        // this.resetValue();
        this.validateForm.get('ParentId')?.setValue(undefined);
    }

    async chooseBlock() {
        await new Promise(f => setTimeout(f, 50));
        let id = this.validateForm.value.ParentId;

        if (id) {
            const resp = await this.blockRepository.getById(id);
            if (resp.meta?.error_code == 200) {
                let block = resp.data;

                this.validateForm.get('ParentTypeReportApply')?.setValue(block.TypeReportApply);
                this.validateForm.get('Address')?.setValue(block.Address);
                this.validateForm.get('Lane')?.setValue(block.Lane);
                this.validateForm.get('Ward')?.setValue(block.Ward);
                this.validateForm.get('District')?.setValue(block.District);
                this.validateForm.get('Province')?.setValue(block.Province);
                this.validateForm.get('Pdw')?.setValue([block.Province, block.District, block.Ward]);
                this.validateForm.get('TypePile')?.setValue(block.TypePile);
                this.validateForm.get('TypeBlockId')?.setValue(block.TypeBlockId);
                this.validateForm.get('FloorBlockMap')?.setValue(block.FloorBlockMap);
                this.validateForm.get('ConstructionAreaValue')?.setValue(block.ConstructionAreaValue);
                this.validateForm.get('ConstructionAreaNote')?.setValue(block.ConstructionAreaNote);
                this.validateForm.get('SellConstructionAreaValue')?.setValue(block.SellConstructionAreaValue);
                this.validateForm.get('SellConstructionAreaNote')?.setValue(block.SellConstructionAreaNote);
                this.validateForm.get('UseAreaValue')?.setValue(block.UseAreaValue);
                this.validateForm.get('UseAreaNote')?.setValue(block.UseAreaNote);
                this.validateForm.get('LandUsePlanningInfo')?.setValue(block.LandUsePlanningInfo);
                this.validateForm.get('HighwayPlanningInfo')?.setValue(block.HighwayPlanningInfo);
                this.validateForm.get('LandAcquisitionSituationInfo')?.setValue(block.LandAcquisitionSituationInfo);
                this.validateForm.get('LandNo')?.setValue(block.LandNo);
                this.validateForm.get('MapNo')?.setValue(block.MapNo);
                this.validateForm.get('Width')?.setValue(block.Width);
                this.validateForm.get('Deep')?.setValue(block.Deep);
                this.validateForm.get('LandPriceItemId_99')?.setValue(block.LandPriceItemId_99);
                this.validateForm.get('LandPriceItemValue_99')?.setValue(block.LandPriceItemValue_99);
                this.validateForm.get('LandPriceItemId_34')?.setValue(block.LandPriceItemId_34);
                this.validateForm.get('LandPriceItemValue_34')?.setValue(block.LandPriceItemValue_34);
                this.validateForm.get('PositionCoefficientId_99')?.setValue(block.PositionCoefficientId_99);
                this.validateForm.get('PositionCoefficientStr_99')?.setValue(block.PositionCoefficientStr_99);
                this.validateForm.get('LandscapeLocation_99')?.setValue(block.LandscapeLocation_99 ? block.LandscapeLocation_99.toString() : undefined);
                this.validateForm.get('LandPriceRefinement_99')?.setValue(block.LandPriceRefinement_99);
                this.validateForm.get('LandScapePrice_99')?.setValue(block.LandScapePrice_99);
                this.validateForm.get('PositionCoefficientId_34')?.setValue(block.PositionCoefficientId_34);
                this.validateForm.get('PositionCoefficientStr_34')?.setValue(block.PositionCoefficientStr_34);
                this.validateForm.get('LandscapeLocation_34')?.setValue(block.LandscapeLocation_34 ? block.LandscapeLocation_34.toString() : undefined);
                this.validateForm.get('LandPriceRefinement_34')?.setValue(block.LandPriceRefinement_34);
                this.validateForm.get('LandScapePrice_34')?.setValue(block.LandScapePrice_34);
                this.validateForm.get('LevelAlley_34')?.setValue(block.LevelAlley_34 ? block.LevelAlley_34.toString() : undefined);
                this.validateForm.get('LandscapeLocationInAlley_34')?.setValue(block.LandscapeLocationInAlley_34 ? block.LandscapeLocationInAlley_34.toString() : undefined);
                this.validateForm.get('IsAlley_34')?.setValue(block.IsAlley_34);
                this.validateForm.get('AlleyPositionCoefficientId_34')?.setValue(block.AlleyPositionCoefficientId_34);
                this.validateForm.get('AlleyPositionCoefficientStr_34')?.setValue(block.AlleyPositionCoefficientStr_34);
                this.validateForm.get('AlleyLandScapePrice_34')?.setValue(block.AlleyLandScapePrice_34);
                this.validateForm.get('TextBasedInfo')?.setValue(block.TextBasedInfo);
                this.validateForm.get('CaseApply_34')?.setValue(block.CaseApply_34 ? block.CaseApply_34.toString() : undefined);
                this.validateForm.get('TypeBlockEntity')?.setValue(TypeBlockEntityEnum.BLOCK_NORMAL);
                this.validateForm.get('ApprovedForConstructionOnTheApartmentYard')?.setValue(block.ApprovedForConstructionOnTheApartmentYard);
                this.validateForm.get('blockMaintextureRaties')?.setValue(
                    block.blockMaintextureRaties.map((item: any) => {
                        delete item.Id;
                        return item;
                    })
                );
                this.validateForm.get('levelBlockMaps')?.setValue(
                    block.levelBlockMaps.map((item: any) => {
                        delete item.Id;
                        return item;
                    })
                );

                this.eventEmitterChooseBlock.emit();
            }
        }
        else {
            this.resetValue();
        }

    }

    resetValue() {

        this.validateForm.get('ParentTypeReportApply')?.setValue(undefined);
        this.validateForm.get('Address')?.setValue(undefined);
        this.validateForm.get('Lane')?.setValue(undefined);
        this.validateForm.get('Ward')?.setValue(undefined);
        this.validateForm.get('District')?.setValue(undefined);
        this.validateForm.get('Province')?.setValue(undefined);
        this.validateForm.get('Pdw')?.setValue([]);
        this.validateForm.get('TypePile')?.setValue(undefined);
        this.validateForm.get('TypeBlockId')?.setValue(undefined);
        this.validateForm.get('FloorBlockMap')?.setValue(undefined);
        this.validateForm.get('ConstructionAreaValue')?.setValue(undefined);
        this.validateForm.get('ConstructionAreaNote')?.setValue(undefined);
        this.validateForm.get('SellConstructionAreaValue')?.setValue(undefined);
        this.validateForm.get('SellConstructionAreaNote')?.setValue(undefined);
        this.validateForm.get('UseAreaValue')?.setValue(undefined);
        this.validateForm.get('UseAreaNote')?.setValue(undefined);
        this.validateForm.get('TypePile')?.setValue(undefined);
        this.validateForm.get('LandUsePlanningInfo')?.setValue(undefined);
        this.validateForm.get('HighwayPlanningInfo')?.setValue(undefined);
        this.validateForm.get('LandAcquisitionSituationInfo')?.setValue(undefined);
        this.validateForm.get('LandNo')?.setValue(undefined);
        this.validateForm.get('MapNo')?.setValue(undefined);
        this.validateForm.get('Width')?.setValue(undefined);
        this.validateForm.get('Deep')?.setValue(undefined);
        this.validateForm.get('LandPriceItemId_99')?.setValue(undefined);
        this.validateForm.get('LandPriceItemValue_99')?.setValue(undefined);
        this.validateForm.get('LandPriceItemId_34')?.setValue(undefined);
        this.validateForm.get('LandPriceItemValue_34')?.setValue(undefined);
        this.validateForm.get('PositionCoefficientId_99')?.setValue(undefined);
        this.validateForm.get('PositionCoefficientStr_99')?.setValue(undefined);
        this.validateForm.get('LandscapeLocation_99')?.setValue(undefined);
        this.validateForm.get('LandPriceRefinement_99')?.setValue(undefined);
        this.validateForm.get('LandScapePrice_99')?.setValue(undefined);
        this.validateForm.get('PositionCoefficientId_34')?.setValue(undefined);
        this.validateForm.get('PositionCoefficientStr_34')?.setValue(undefined);
        this.validateForm.get('LandscapeLocation_34')?.setValue(undefined);
        this.validateForm.get('LandPriceRefinement_34')?.setValue(undefined);
        this.validateForm.get('LandScapePrice_34')?.setValue(undefined);
        this.validateForm.get('LevelAlley_34')?.setValue(undefined);
        this.validateForm.get('LandscapeLocationInAlley_34')?.setValue(undefined);
        this.validateForm.get('IsAlley_34')?.setValue(undefined);
        this.validateForm.get('AlleyPositionCoefficientId_34')?.setValue(undefined);
        this.validateForm.get('AlleyPositionCoefficientStr_34')?.setValue(undefined);
        this.validateForm.get('AlleyLandScapePrice_34')?.setValue(undefined);
        this.validateForm.get('TextBasedInfo')?.setValue(undefined);
        this.validateForm.get('CaseApply_34')?.setValue(undefined);
        this.validateForm.get('TypeBlockEntity')?.setValue(TypeBlockEntityEnum.BLOCK_NORMAL);
        this.validateForm.get('ApprovedForConstructionOnTheApartmentYard')?.setValue(undefined);
        this.validateForm.get('levelBlockMaps')?.setValue([]);
        this.validateForm.get('blockMaintextureRaties')?.setValue(undefined);

        this.eventEmitterChooseBlock.emit();
    }
}
