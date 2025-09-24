import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { TypeApartmentLandDetailEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-block-landscape-area-info',
    templateUrl: './landscape-area-info.component.html'
})

export class LandscapeAreaInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;

    typeApartmentLandDetailEnum = TypeApartmentLandDetailEnum;

    constructor(
    ) { }

    ngOnInit(): void {
    }

    changeApartmentLandDetail(apartmentLandDetails: any) {
        this.validateForm.get('apartmentLandDetails')?.setValue(apartmentLandDetails);
        this.validateForm.get('LandscapePrivateAreaValue')?.setValue(apartmentLandDetails.reduce((x: number, curItem: any) => { return x + curItem.PrivateArea }, 0));
    }
}
