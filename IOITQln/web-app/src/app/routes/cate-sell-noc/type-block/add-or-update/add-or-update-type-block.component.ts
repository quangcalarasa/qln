import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { AccessKey, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-type-block',
    templateUrl: './add-or-update-type-block.component.html'
})

export class AddOrUpdateTypeBlockComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;

    typeReportApplyData = TypeReportApply;
    TypeReportApplyEnum = TypeReportApplyEnum;
    role = this.commonService.CheckAccessKeyRole(AccessKey.TYPE_BLOCK_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private typeBlockRepository: TypeBlockRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            typeBlockMaps: [this.record ? this.record.typeBlockMaps : undefined, [Validators.required]]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        if (data.typeBlockMaps) {
            data.typeBlockMaps.forEach((x: any) => {
                x.TypeReportApply = x.TypeReportApply ?? x.key;

                return x;
            });
        }

        const resp = data.Id ? await this.typeBlockRepository.update(data) : await this.typeBlockRepository.addNew(data);
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

    compareFn = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.key === o2.key || o1.TypeReportApply === parseInt(o2.key) : o1 === o2);
    };
}
