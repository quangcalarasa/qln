import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AreaCorrectionCoefficientRepository } from 'src/app/infrastructure/repositories/area-correction-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { convertDate } from 'src/app/infrastructure/utils/common';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-area-correction-coefficient',
    templateUrl: './add-or-update-area-correction-coefficient.component.html'
})

export class AddOrUpdateAreaCorrectionCoefficientComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    parent_data: any[] = [];
    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.AREA_CORRECTION_COEFFICIENT_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private areaCorrectionCoefficientRepository: AreaCorrectionCoefficientRepository, private cdr: ChangeDetectorRef, private message: NzMessageService, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, [Validators.required]],
            ParentId: [this.record ? this.record.ParentId : undefined, []],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, [Validators.required]],
            Value: [this.record ? this.record.Value : undefined, [Validators.required]],
            DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, []]
        });

        this.getDataParent();
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.areaCorrectionCoefficientRepository.update(data) : await this.areaCorrectionCoefficientRepository.addNew(data);
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

    async getDataParent() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = this.record ? `ParentId=null AND Id!=${this.record.Id}` : 'ParentId=null';
        paging.select = "Id,Name";

        const resp = await this.areaCorrectionCoefficientRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.parent_data = resp.data;
        }
    }
}
