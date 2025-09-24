import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DecreeRepository } from 'src/app/infrastructure/repositories/decree.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-add-or-update-decree',
    templateUrl: './add-or-update-decree.component.html'
})

export class AddOrUpdateDecreeComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() typeDecree: NzSafeAny;
    nzFormat = 'dd/ MM/ yyyy';
    role = this.commonService.CheckAccessKeyRole(AccessKey.DECREE_TYPE2_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private decreeRepository: DecreeRepository, private cdr: ChangeDetectorRef,
        private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            TypeDecree: [this.record ? this.record.TypeDecree : this.typeDecree],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            DoPub: [this.record ? convertDate(this.record.DoPub) : undefined, []],
            DecisionUnit: [this.record ? this.record.DecisionUnit : undefined, []],
            Note: [this.record ? this.record.Note : undefined, [Validators.required]],
            ApplyCalculateRentalPrice: [this.record ? this.record.ApplyCalculateRentalPrice : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.decreeRepository.update(data, this.typeDecree) : await this.decreeRepository.addNew(data, this.typeDecree);
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
