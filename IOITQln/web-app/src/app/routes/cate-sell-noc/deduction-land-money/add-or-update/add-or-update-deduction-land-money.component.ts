import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DeductionLandMoneyRepository } from 'src/app/infrastructure/repositories/deduction-land-money.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-deduction-land-money',
    templateUrl: './add-or-update-deduction-land-money.component.html'
})

export class AddOrUpdateDeductionLandMoneyComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.DEDUCTION_LAND_MONEY_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private deductionLandMoneyRepository: DeductionLandMoneyRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            Condition: [this.record ? this.record.Condition : undefined, [Validators.required]],
            Value: [this.record ? this.record.Value : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.deductionLandMoneyRepository.update(data) : await this.deductionLandMoneyRepository.addNew(data);
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
