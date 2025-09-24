import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-shared-popover',
    templateUrl: './popover.component.html',
    styles: []
})

export class SharedPopoverComponent implements OnInit {
    @Input() cs?: number;
    constructor(
    ) { }

    ngOnInit(): void {
    }
}