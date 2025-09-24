import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TemplateRepository } from 'src/app/infrastructure/repositories/template.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';

@Component({
    selector: 'app-add-or-update-template',
    templateUrl: './add-or-update-template.component.html'
})

export class AddOrUpdateTemplateComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() template_groups: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private templateRepository: TemplateRepository, private modalSrv: NzModalService, private uploadRepository: UploadRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Attactment: [this.record ? this.record.Attactment : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined],
            Type: [this.record ? this.record.Type : undefined, [Validators.required]]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.templateRepository.update(data) : await this.templateRepository.addNew(data);
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

    beforeUpload = (file: NzUploadFile): boolean => {
        this.handleChange(file);
        return false;
    };

    async handleChange(file: any) {
        const formData = new FormData();
        formData.append(file.name, file);

        const resp = await this.uploadRepository.uploadFile(formData);
        this.validateForm.get('Attactment')?.setValue(resp?.data.toString());
    }

    removeAttactment() {
        this.validateForm.get('Attactment')?.setValue(undefined);
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}
