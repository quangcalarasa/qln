import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { TypeReportApplyEnum, DecreeEnum, TypeBlockEntityEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-rent-block-address',
    templateUrl: './rent-block-address.component.html'
})

export class RentBlockAddressComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;

    lane_data: any[] = [];
    pdw_data = [];

    constructor(
        private provinceRepository: ProvinceRepository, private cdr: ChangeDetectorRef, private laneRepository: LaneRepository
    ) { }

    ngOnInit(): void {
        this.getCascaderData();

        if (this.validateForm.value.Ward) {
            this.getLaneData(this.validateForm.value.Ward, true);
        }
    }

    async getCascaderData() {
        const resp = await this.provinceRepository.getCascaderData(1);

        if (resp.meta?.error_code == 200) {
            this.pdw_data = resp.data;
        }
    }

    changePdw(pdw: any) {
        if (pdw.length == 0) {
            this.validateForm.value.Province = undefined;
            this.validateForm.value.District = undefined;
            this.validateForm.value.Ward = undefined;

            this.getLaneData(undefined, false);
            this.eventEmitter.emit();
        }
        else {
            this.validateForm.get('Province')?.setValue(pdw[0]);
            this.validateForm.get('District')?.setValue(pdw[1]);
            this.validateForm.get('Ward')?.setValue(pdw[2]);

            this.getLaneData(pdw[2], false);

            if (this.validateForm.value.TypeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)

                this.eventEmitter.emit();
        }

        this.cdr.detectChanges();
    }

    //danh sách đường theo WardId
    async getLaneData(wardId?: number, init: boolean = true) {
        // if (!init) this.validateForm.get('Lane')?.setValue(undefined);
        this.lane_data = [];
        if (!wardId) return;

        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = `Ward=${wardId}`;

        const resp = await this.laneRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.lane_data = resp.data;

            if (resp.metadata == 1 && !init) {
                this.validateForm.get('Lane')?.setValue(this.lane_data[0].Id);
            }
        }
    }

    resetLaneValue() {
        // console.log("HERE!");
        this.validateForm.get('Lane')?.setValue(undefined);
    }
}
