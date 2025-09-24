import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { LevelBlock } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { PricingRepository } from 'src/app/infrastructure/repositories/pricing.repository';
import { ProcessProfileCeRepository } from 'src/app/infrastructure/repositories/process-profile-ce.repository';

@Component({
    selector: 'app-pricing-apartment-common-info',
    templateUrl: './common-info.component.html'
})

export class ApartmentCommonInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() block: any;
    @Input() typehouse_data: NzSafeAny;

    pricing_data: any[] = [];
    process_profile_ce_data: any[] = [];
    TypeReportApplyEnum = TypeReportApplyEnum;

    constructor(
        private pricingRepository: PricingRepository,
        private processProfileCeRepository: ProcessProfileCeRepository
    ) { }

    ngOnInit(): void {
        this.getPricingReplaceds();
        this.getProcessProfileCeData();
    }

    getByDecreeAndDate() {
        this.eventEmitter.emit();
    }

    async getPricingReplaceds() {
        this.pricing_data = [];

        let pricingId = this.validateForm.value.Id;
        let typeReportApply = this.validateForm.value.TypeReportApply;
        if (typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            let blockId = this.validateForm.value.BlockId;

            if (blockId) {
                let paging: GetByPageModel = new GetByPageModel();
                paging.page_size = 0;
                paging.query = `BlockId=${blockId}`;

                if (pricingId)
                    paging.query += ` AND Id!=${pricingId}`;

                paging.select = "Id,DateCreate";
                paging.order_by = "DateCreate Desc";

                const resp = await this.pricingRepository.getByPage(paging);

                if (resp.meta?.error_code == 200) {
                    this.pricing_data = resp.data;
                }
            }
        }
        else {
            let apartmentId = this.validateForm.value.ApartmentId;

            if (apartmentId) {
                let paging: GetByPageModel = new GetByPageModel();
                paging.page_size = 0;
                paging.query = `ApartmentId=${apartmentId}`;

                if (pricingId)
                    paging.query += ` AND Id!=${pricingId}`;

                paging.select = "Id,DateCreate";
                paging.order_by = "DateCreate Desc";

                const resp = await this.pricingRepository.getByPage(paging);

                if (resp.meta?.error_code == 200) {
                    this.pricing_data = resp.data;
                }
            }
        }
    }

    compareFn = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.Id === o2.Id || o1.PricingReplacedId === o2.Id : o1 === o2);
    };

    async getProcessProfileCeData() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;

        paging.select = "Code";

        const resp = await this.processProfileCeRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.process_profile_ce_data = resp.data;
        }
    }
}
