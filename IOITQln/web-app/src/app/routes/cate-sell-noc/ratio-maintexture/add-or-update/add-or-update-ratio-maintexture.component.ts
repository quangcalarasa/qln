import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CurrentStateMainTextureRepository } from 'src/app/infrastructure/repositories/ct-maintexture.repository';
import { RatioMainTextureRepository } from 'src/app/infrastructure/repositories/ratio-maintexture.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeMainTexTure } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-ratio-maintexture',
    templateUrl: './add-or-update-ratio-maintexture.component.html'
})

export class AddOrUpdateRatioMainTextureComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;

    typemaintexture_data = TypeMainTexTure;

    data_parent: any[] = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.RATIO_MAINTEXTURE_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private currentStateMainTextureRepository: CurrentStateMainTextureRepository, private cdr: ChangeDetectorRef,
        private ratioMainTextureRepository: RatioMainTextureRepository, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            ParentId: [this.record ? this.record.ParentId : undefined, []],
            // Code: [this.record ? this.record.Code : undefined, []],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            TypeMainTexTure1: [this.record ? this.record.TypeMainTexTure1 : undefined, []],
            TypeMainTexTure2: [this.record ? this.record.TypeMainTexTure2 : undefined, []],
            TypeMainTexTure3: [this.record ? this.record.TypeMainTexTure3 : undefined, []],
            TypeMainTexTure4: [this.record ? this.record.TypeMainTexTure4 : undefined, []],
            TypeMainTexTure5: [this.record ? this.record.TypeMainTexTure5 : undefined, []],
            TypeMainTexTure6: [this.record ? this.record.TypeMainTexTure6 : undefined, []]
        });

        this.getDataParent();
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.ratioMainTextureRepository.update(data) : await this.ratioMainTextureRepository.addNew(data);
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
        paging.query = 'ParentId=null';
        paging.select = "Id,Code,Name";

        const resp = await this.ratioMainTextureRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.data_parent = resp.data;
        }
    }
}
