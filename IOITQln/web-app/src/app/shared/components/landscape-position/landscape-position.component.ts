import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { DecreeEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-shared-component-landscape-position',
    templateUrl: './landscape-position.component.html'
})

export class LandscapePositionComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() landprice_data: any[] = [];

    DecreeEnum = DecreeEnum;

    constructor(
        private commonService: CommonService
    ) { }

    ngOnInit(): void {
    }

    checkDecree(decree: DecreeEnum) {
        let decreeMaps = this.validateForm.value.decreeMaps;

        return this.commonService.checkDecree(decree, decreeMaps);
    }

    viewNameDecree(decree: DecreeEnum) {
        return this.commonService.viewNameDecree(decree);
    }

    changeLandPriceItem(event: any, decree: DecreeEnum) {
        let landprice_data = this.landprice_data.find(x => x.Id == event);

        switch (decree) {
            case DecreeEnum.ND_99:
                this.validateForm.get('LandPriceItemValue_99')?.setValue(landprice_data ? landprice_data.Value : undefined);
                break;
            case DecreeEnum.ND_34:
                this.validateForm.get('LandPriceItemValue_34')?.setValue(landprice_data ? landprice_data.Value : undefined);
                break;
            case DecreeEnum.ND_61:
                this.validateForm.get('LandPriceItemValue_61')?.setValue(landprice_data ? landprice_data.Value : undefined);
                break;
            default: break;
        }

        this.eventEmitter.emit(decree);
        // this.changePositionCoefficientId(decree);
        // if (decree == DecreeEnum.ND_34) this.changePositionCoefficientId(DecreeEnum.SPECIAL);
    }
}
