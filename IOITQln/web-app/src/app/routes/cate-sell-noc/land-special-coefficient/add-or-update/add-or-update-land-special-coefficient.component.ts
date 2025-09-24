import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LandSpecialCoefficientRepository } from 'src/app/infrastructure/repositories/land-special-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-land-special-coefficient',
    templateUrl: './add-or-update-land-special-coefficient.component.html'
})

export class AddOrUpdateLandSpecialCoefficientComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.LAND_SPECIAL_COEFFICIENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private landSpecialCoefficientRepository: LandSpecialCoefficientRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, []],
            Value1: [this.record ? this.record.Value1 : undefined, [Validators.required]],
            Value2: [this.record ? this.record.Value2 : undefined, [Validators.required]],
            Value3: [this.record ? this.record.Value3 : undefined, [Validators.required]],
            Value4: [this.record ? this.record.Value4 : undefined, [Validators.required]],
            Value5: [this.record ? this.record.Value5 : undefined, [Validators.required]],
            Value6: [this.record ? this.record.Value6 : undefined, [Validators.required]],
            Value7: [this.record ? this.record.Value7 : undefined, [Validators.required]],
            Value8: [this.record ? this.record.Value8 : undefined, [Validators.required]]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.landSpecialCoefficientRepository.update(data) : await this.landSpecialCoefficientRepository.addNew(data);
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
