import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CurrentStateMainTextureRepository } from 'src/app/infrastructure/repositories/ct-maintexture.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { TypeMainTexTure, LevelBlock } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-add-or-update-currstate-maintexture',
    templateUrl: './add-or-update-currstate-maintexture.component.html'
})

export class AddOrUpdateCurrentStateMainTextureComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;

    typemaintexture_data = TypeMainTexTure;
    levelblock_data = LevelBlock;
    role = this.commonService.CheckAccessKeyRole(AccessKey.CURRENTSTATE_MAINTEXTURE_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private currentStateMainTextureRepository: CurrentStateMainTextureRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            LevelBlock: [this.record ? this.record.LevelBlock.toString() : undefined, [Validators.required]],
            TypeMainTexTure: [this.record ? this.record.TypeMainTexTure.toString() : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Default: [this.record ? this.record.Default : undefined, []],
            Note: [this.record ? this.record.Note : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.currentStateMainTextureRepository.update(data) : await this.currentStateMainTextureRepository.addNew(data);
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
