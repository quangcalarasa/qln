import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PositionCoefficientRepository } from 'src/app/infrastructure/repositories/position-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-position-coefficient',
    templateUrl: './add-or-update-position-coefficient.component.html'
})

export class AddOrUpdatePositionCoefficientComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.POSITION_COEFFICIENT_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private positionCoefficientRepository: PositionCoefficientRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, []],
            LocationValue1: [this.record ? this.record.LocationValue1 : undefined, [Validators.required]],
            LocationValue2: [this.record ? this.record.LocationValue2 : undefined, [Validators.required]],
            LocationValue3: [this.record ? this.record.LocationValue3 : undefined, [Validators.required]],
            LocationValue4: [this.record ? this.record.LocationValue4 : undefined, [Validators.required]],
            AlleyValue1: [this.record ? this.record.AlleyValue1 : undefined, [Validators.required]],
            AlleyValue2: [this.record ? this.record.AlleyValue2 : undefined, [Validators.required]],
            AlleyValue3: [this.record ? this.record.AlleyValue3 : undefined, [Validators.required]],
            AlleyValue4: [this.record ? this.record.AlleyValue4 : undefined, [Validators.required]],
            AlleyLevel2: [this.record ? this.record.AlleyLevel2 : undefined, []],
            AlleyOther: [this.record ? this.record.AlleyOther : undefined, []],
            AlleyLand: [this.record ? this.record.AlleyLand : undefined, []],
            PositionDeep: [this.record ? this.record.PositionDeep : undefined, []],
            LandPriceRefinement: [this.record ? this.record.LandPriceRefinement : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.positionCoefficientRepository.update(data) : await this.positionCoefficientRepository.addNew(data);
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
