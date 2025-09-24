import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'app-block-landscape-info',
    templateUrl: './landscape-info.component.html'
})

export class LandscapeInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;

    constructor(
    ) { }

    ngOnInit(): void {
    }
}
