import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeReportApply, LevelBlock, Decree } from 'src/app/shared/utils/consts';
import { TypeBlockEntityEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';

@Component({
    selector: 'app-cate-apartment-range',
    templateUrl: './apartment-range.component.html'
})

export class ApartmentRangeComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() block?: NzSafeAny;

    block_data: any[] = [];

    typeReportApply_data = TypeReportApply;
    level_data = LevelBlock;
    decree_type1_data = Decree;

    TypeReportApplyEnum = TypeReportApplyEnum;

    constructor(
        private blockRepository: BlockRepository,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        this.getDataBlock(true);

        if (this.validateForm.value.BlockId) {
            this.changeBlock(this.validateForm.value.BlockId, true);
        }
    }

    async getDataBlock(isInit: boolean) {
        this.block_data = [];

        if (!isInit) this.validateForm.get('BlockId')?.setValue(undefined);

        let TypeReportApply = this.validateForm.value.TypeReportApply;

        if (TypeReportApply) {
            let paging: GetByPageModel = new GetByPageModel();
            paging.query = `TypeBlockEntity=${TypeBlockEntityEnum.BLOCK_NORMAL} AND TypeReportApply=${TypeReportApply}`;
            paging.page_size = 0;
            paging.select = 'Id,Address';

            const resp = await this.blockRepository.getByPage(paging);

            if (resp.meta?.error_code == 200) {
                this.block_data = resp.data;
            }
        }
    }

    async changeBlock(event: any, isInit: boolean) {
        this.block = undefined;
        if (event) {
            const resp = await this.blockRepository.getById(event);

            if (resp.meta?.error_code == 200) {
                this.block = resp.data;

                if (!isInit) {
                    if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
                        this.validateForm.get('LandscapeAreaValue1')?.setValue(this.block?.ConstructionAreaValue1);
                        this.calcLandscapeAreaValue();
                    }

                    if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                        this.validateForm.get('ParentTypeReportApply')?.setValue(this.block?.ParentTypeReportApply);
                }

                this.changeFormValidation(this.validateForm.value.TypeReportApply);
            }
        }

        this.targetBlock();
    }

    targetBlock() {
        this.eventEmitter.emit(this.block);
    }

    compareFn = (o1: any, o2: any) => {
        return o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2;
    };

    compareFnDecree = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.key === o2.key || o1.DecreeType1Id === parseInt(o2.key) : o1 === o2);
    };

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

    changeFormValidation(typeReportApply: TypeReportApplyEnum) {
        // console.log("Go to here!");
        if (typeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
            this.validateForm.get('ConstructionAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators(null);
        }
        else if (typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
            this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
            this.validateForm.get('UseAreaValue')?.setValidators([Validators.required]);
        }
        else if (typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) {
            if (this.validateForm.value.ParentTypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
                this.validateForm.get('ConstructionAreaValue')?.setValidators([Validators.required]);
                this.validateForm.get('UseAreaValue')?.setValidators([Validators.required]);
            }
            else {
                this.validateForm.get('ConstructionAreaValue')?.setValidators(null);
                // this.validateForm.get('ConstructionAreaValue')?.updateValueAndValidity();
                this.validateForm.get('UseAreaValue')?.setValidators(null);
            }
        }
        else {
            this.validateForm.get('ConstructionAreaValue')?.setValidators(null);
            this.validateForm.get('UseAreaValue')?.setValidators(null);
        }

        this.validateForm.get('ConstructionAreaValue')?.updateValueAndValidity();
        this.validateForm.get('UseAreaValue')?.updateValueAndValidity();

        // this.cdr.detectChanges();
        // this.validateForm.updateValueAndValidity();

    }
}
