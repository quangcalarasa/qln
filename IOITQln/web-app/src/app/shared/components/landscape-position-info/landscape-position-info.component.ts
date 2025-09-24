import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { DecreeEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';
import { LevelAlley, LocationResidentialLand, TypeAlley, IsAlley_61_Coefficient } from 'src/app/shared/utils/consts';

@Component({
    selector: 'app-shared-component-landscape-position-info',
    templateUrl: './landscape-position-info.component.html'
})

export class LandscapePositionInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() position_coefficient_data: any[] = [];
    @Input() disabled: boolean = true;

    DecreeEnum = DecreeEnum;
    levelalley_data = LevelAlley;
    typeAlley_data = TypeAlley;
    landscapelocation_data = LocationResidentialLand;
    isAlley_61_Coefficient = IsAlley_61_Coefficient;
    TypeReportApplyEnum = TypeReportApplyEnum;

    validateFormRawValue: any;

    constructor(
        private commonService: CommonService
    ) { }

    ngOnInit(): void {
        this.validateFormRawValue = this.validateForm.getRawValue();
    }

    checkDecree(decree: DecreeEnum) {
        let decreeMaps = this.validateForm.value.decreeMaps;

        return this.commonService.checkDecree(decree, decreeMaps);
    }

    viewNameDecree(decree: DecreeEnum) {
        return this.commonService.viewNameDecree(decree);
    }

    changeLandscapeLocation(event: any, decree: DecreeEnum) {
        if (!event) {
            switch (decree) {
                case DecreeEnum.ND_99:
                    this.validateForm.get('PositionCoefficientId_99')?.setValue(undefined);
                    break;
                case DecreeEnum.ND_34:
                    this.validateForm.get('PositionCoefficientId_34')?.setValue(undefined);
                    break;
                case DecreeEnum.ND_61:
                    this.validateForm.get('PositionCoefficientId_61')?.setValue(undefined);
                    break;
                default:
                    this.validateForm.get('AlleyPositionCoefficientId_34')?.setValue(undefined);
                    break;
            }
        }
        else {
            let checkDecree = decree != DecreeEnum.ND_99 && decree != DecreeEnum.ND_61 ? DecreeEnum.ND_34 : decree;
            let position_coefficient_data = this.position_coefficient_data.filter(x => x.DecreeType1Id == checkDecree);

            switch (decree) {
                case DecreeEnum.ND_99:
                    if (position_coefficient_data.length > 0) {
                        this.validateForm.get('PositionCoefficientId_99')?.setValue(position_coefficient_data[0].Id);
                    }
                    else this.validateForm.get('PositionCoefficientId_99')?.setValue(undefined);
                    break;
                case DecreeEnum.ND_34:
                    if (position_coefficient_data.length > 0) {
                        this.validateForm.get('PositionCoefficientId_34')?.setValue(position_coefficient_data[0].Id);
                    }
                    else this.validateForm.get('PositionCoefficientId_34')?.setValue(undefined);
                    break;
                case DecreeEnum.ND_61:
                    if (position_coefficient_data.length > 0) {
                        this.validateForm.get('PositionCoefficientId_61')?.setValue(position_coefficient_data[0].Id);
                    }
                    else this.validateForm.get('PositionCoefficientId_61')?.setValue(undefined);
                    break;
                default:
                    if (position_coefficient_data.length > 0) {
                        this.validateForm.get('AlleyPositionCoefficientId_34')?.setValue(position_coefficient_data[0].Id);
                    }
                    else this.validateForm.get('AlleyPositionCoefficientId_34')?.setValue(undefined);
                    break;
            }
        }

        this.changePositionCoefficientId(decree);
    }

    changePositionCoefficientId(decree: DecreeEnum) {
        this.eventEmitter.emit(decree);
    }

    changeDeepValue(event: any) {
        let decreeMaps = this.validateForm.value.decreeMaps;
        if(this.commonService.checkDecree(DecreeEnum.ND_99, decreeMaps)) {
            this.validateForm.get('LandPriceRefinement_99')?.setValue(event ? 10 : undefined);
        }

        if(this.commonService.checkDecree(DecreeEnum.ND_34, decreeMaps)) {
            this.validateForm.get('LandPriceRefinement_34')?.setValue(event ? 10 : undefined);
        }

        decreeMaps.forEach((decreeMap: any) => {
            let decree = decreeMap.key ?? decreeMap.DecreeType1Id;
            this.changePositionCoefficientId(parseInt(decree));
        });
    }
}
