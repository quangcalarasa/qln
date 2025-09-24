import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LandPriceCorrectionCoefficientRepository } from 'src/app/infrastructure/repositories/landprice-correction-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-landprice-correction-coefficient',
    templateUrl: './add-or-update-landprice-correction-coefficient.component.html'
})

export class AddOrUpdateLandPriceCorrectionCoefficientComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.LAND_PRICE_CORRECTION_COEFFICIENT_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private landPriceCorrectionCoefficientRepository: LandPriceCorrectionCoefficientRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            FacadeWidth: [this.record ? this.record.FacadeWidth : undefined, [Validators.required]],
            Value: [this.record ? this.record.Value : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.landPriceCorrectionCoefficientRepository.update(data) : await this.landPriceCorrectionCoefficientRepository.addNew(data);
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
