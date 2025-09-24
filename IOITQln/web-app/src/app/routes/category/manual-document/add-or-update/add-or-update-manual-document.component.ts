import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ManualDocumentRepository } from 'src/app/infrastructure/repositories/manual-document.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { ModuleSystem } from 'src/app/shared/utils/consts';

@Component({
    selector: 'app-add-or-update-manual-document',
    templateUrl: './add-or-update-manual-document.component.html'
})

export class AddOrUpdateManualDocumentComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    dataModuleSystem = ModuleSystem;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private manualDocumentRepository: ManualDocumentRepository, private modalSrv: NzModalService, private uploadRepository: UploadRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Title: [this.record ? this.record.Title : undefined, [Validators.required]],
            Attactment: [this.record ? this.record.Attactment : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined],
            Type: [this.record ? (this.record.Type ? this.record.Type.toString() : undefined) : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.manualDocumentRepository.update(data) : await this.manualDocumentRepository.addNew(data);
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
