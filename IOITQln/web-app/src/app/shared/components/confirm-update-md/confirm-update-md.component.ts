import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';

@Component({
    selector: 'app-shared-confirm-update-md',
    templateUrl: './confirm-update-md.component.html'
})
export class SharedConfirmUpdateMdComponent implements OnInit {
    validateForm!: FormGroup;

    constructor(
        private fb: FormBuilder,
        private modal: NzModalRef,
        private uploadRepository: UploadRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            ContentUpdate: [undefined, [Validators.required]],
            ReasonUpdate: [undefined, [Validators.required]],
            AttactmentUpdate: [undefined, [Validators.required]]
            // AttactmentUpdate: [undefined, []]
        });
    }

    async submitForm() {
        this.modal.triggerOk();
    }

    close(): void {
        this.modal.close();
    }

    beforeUpload = (file: NzUploadFile): boolean => {
        this.handleChange(file);
        return false;
    };

    async handleChange(file: any) {
        const formData = new FormData();
        formData.append(file.name, file);

        const resp = await this.uploadRepository.uploadFile(formData);
        this.validateForm.get('AttactmentUpdate')?.setValue(resp?.data.toString());
    }

    removeAttactment() {
        this.validateForm.get('AttactmentUpdate')?.setValue(undefined);
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}
