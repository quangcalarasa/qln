import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-block-block-area',
    templateUrl: './block-area.component.html'
})

export class BlockAreaComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;

    TypeReportApplyEnum = TypeReportApplyEnum;

    constructor(
    ) { }

    ngOnInit(): void {
    }

    calcConstructionAreaValue() {
        let constructionAreaValue1 = this.validateForm.value.ConstructionAreaValue1;
        let constructionAreaValue2 = this.validateForm.value.ConstructionAreaValue2;
        let constructionAreaValue3 = this.validateForm.value.ConstructionAreaValue3;

        if (!constructionAreaValue1 && !constructionAreaValue2 && !constructionAreaValue3) {
            this.validateForm.get('ConstructionAreaValue')?.setValue(undefined);
        }
        else {
            let constructionAreaValue = (constructionAreaValue1 ?? 0) + (constructionAreaValue2 ?? 0) + (constructionAreaValue3 ?? 0);
            this.validateForm.get('ConstructionAreaValue')?.setValue(Math.round(constructionAreaValue * 100) / 100);
        }
    }

    calcUseAreaValue() {
        let useAreaValue1 = this.validateForm.value.UseAreaValue1;
        let useAreaValue2 = this.validateForm.value.UseAreaValue2;

        if (!useAreaValue1 && !useAreaValue2) {
            this.validateForm.get('UseAreaValue')?.setValue(undefined);
        }
        else {
            let useAreaValue = (useAreaValue1 ?? 0) + (useAreaValue2 ?? 0);
            this.validateForm.get('UseAreaValue')?.setValue(Math.round(useAreaValue * 100) / 100);
        }
    }
}
