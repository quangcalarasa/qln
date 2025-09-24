import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { No2LandPriceRepository } from 'src/app/infrastructure/repositories/no2-land-price.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-no2-land-price',
    templateUrl: './add-or-update-no2-land-price.component.html'
})

export class AddOrUpdateNo2LandPriceComponent implements OnInit {
    @Input() record: NzSafeAny;

    validateForm!: FormGroup;
    loading: boolean = false;
    role = this.commonService.CheckAccessKeyRole(AccessKey.NO2_LAND_PRICE_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private no2LandPriceRepository: No2LandPriceRepository, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            StartValue: [this.record ? this.record.StartValue : undefined, [Validators.required]],
            EndValue: [this.record ? this.record.EndValue : undefined, []],
            MainPriceLess2M: [this.record ? this.record.MainPriceLess2M : undefined, []],
            ExtraPriceLess2M: [this.record ? this.record.ExtraPriceLess2M : undefined, []],
            MainPriceLess3M: [this.record ? this.record.MainPriceLess3M : undefined, []],
            ExtraPriceLess3M: [this.record ? this.record.ExtraPriceLess3M : undefined, []],
            MainPriceLess5M: [this.record ? this.record.MainPriceLess5M : undefined, []],
            ExtraPriceLess5M: [this.record ? this.record.ExtraPriceLess5M : undefined, []],
            MainPriceGreater5M: [this.record ? this.record.MainPriceGreater5M : undefined, []],
            ExtraPriceGreater5M: [this.record ? this.record.ExtraPriceGreater5M : undefined, []],
            Note: [this.record ? this.record.Note : undefined, [Validators.required]],
            TypeUrban: [this.record ? this.record.TypeUrban : undefined, []]
        });
    }

    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.no2LandPriceRepository.update(data) : await this.no2LandPriceRepository.addNew(data);
        if (resp.meta?.error_code == 200) {
            this.loading = false;
            this.drawerRef.close(data);
        }
        else {
            this.loading = false;
        }
    }

    close(): void {
        this.drawerRef.close();
    }
}
