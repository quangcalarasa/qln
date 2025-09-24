import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Decree, TermApply } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum, DecreeEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-shared-adjacent-land',
    templateUrl: './adjacent-land.component.html'
})

export class AdjacentLandSharedComponent implements OnInit {
    @Input() validateForm: any;

    constructor(private message: NzMessageService
    ) { }

    ngOnInit(): void {
    }
}
