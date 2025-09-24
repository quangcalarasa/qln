import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { DecreeEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';
import { DistributionFloorCoefficientRepository } from 'src/app/infrastructure/repositories/distribution-floor-coefficient.repository';

@Component({
    selector: 'app-pricing-flat-coefficient',
    templateUrl: './flat-coefficient.component.html'
})

export class FlatCoefficientComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() decreeMaps: any[] = [];

    flat_coefficient_data: any[] = [];

    DecreeEnum = DecreeEnum;

    constructor(
        private commonService: CommonService,
        private distributionFloorCoefficientRepository: DistributionFloorCoefficientRepository
    ) { }

    ngOnInit(): void {
        this.getFlatCoefficientData();
    }

    checkDecree(decree: DecreeEnum) {
        return this.commonService.checkDecree(decree, this.decreeMaps);
    }

    viewNameDecree(decree: DecreeEnum) {
        return this.commonService.viewNameDecree(decree);
    }

    changeFlatCoefficient(event: any, decree: DecreeEnum) {
        let flat_coefficient = this.flat_coefficient_data.find(x => x.Id == event);

        switch (decree) {
            case DecreeEnum.ND_99:
                this.validateForm.get('FlatCoefficient_99')?.setValue(flat_coefficient ? flat_coefficient.FlatCoefficient : undefined);
                break;
            case DecreeEnum.ND_34:
                this.validateForm.get('FlatCoefficient_34')?.setValue(flat_coefficient ? flat_coefficient.FlatCoefficient : undefined);
                break;
            case DecreeEnum.ND_61:
                this.validateForm.get('FlatCoefficient_61')?.setValue(flat_coefficient ? flat_coefficient.FlatCoefficient : undefined);
                break;
            default: break;
        }

        this.eventEmitter.emit();
    }

    async getFlatCoefficientData() {
        const resp = await this.distributionFloorCoefficientRepository.getFlatCoefficientData();

        if (resp.meta?.error_code == 200) {
            this.flat_coefficient_data = resp.data;
        }
    }
}
