import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { TypeLandSpecial } from 'src/app/shared/utils/consts';

@Component({
    selector: 'app-shared-component-land-special',
    templateUrl: './land-special.component.html'
})

export class BlockLandSpecialComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    landpositionspeical_data = TypeLandSpecial;   //ds thửa đất có hình dạng đặc biệt

    constructor(
    ) { }

    ngOnInit(): void {
    }
}
