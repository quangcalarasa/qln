import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { LevelBlock } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
    selector: 'app-pricing-block-apartment-info',
    templateUrl: './block-apartment-info.component.html'
})

export class PricingBlockApartmentInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() block: any;
    @Input() apartment: any;
    @Input() customer_data: any[] = [];

    TypeReportApplyEnum = TypeReportApplyEnum;

    constructor(
    ) { }

    ngOnInit(): void {
    }

    compareFnCustomer = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.CustomerId === o2.Id : o1 === o2);
    };
}
