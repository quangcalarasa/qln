import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { CodeStatusEnum } from 'src/app/shared/utils/enums';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';

@Component({
    selector: 'app-block-search-rent-apartment',
    templateUrl: './search-rent-apartment.component.html'
})
export class SearchRentApartmentComponent implements OnInit {
    validateForm!: FormGroup;

    data: any;

    codeStatusEnum = CodeStatusEnum;
    codeStatus?: CodeStatusEnum; //Trạng thái của mã định danh
    msg: string = "Mã định danh chưa tồn tại!";
    block?: any;

    constructor(
        private fb: FormBuilder,
        private modal: NzModalRef,
        private apartmentRepository: ApartmentRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Code: [undefined, [Validators.required]]
        });
    }

    async submitForm() {
        // this.data = { ...this.validateForm.value };

        this.modal.triggerOk();
    }

    close(): void {
        this.modal.close();
    }

    async search() {
        this.validateForm.get('TypeReportApply')?.setValue(undefined);

        let data = {
            Code: this.validateForm.value.Code
        }

        const resp = await this.apartmentRepository.checkCodeStatus(data);

        if (resp.meta?.error_code == 200) {
            let data = resp.data;
            this.codeStatus = data.CodeStatus;

            switch (this.codeStatus) {
                case CodeStatusEnum.CHUA_TON_TAI:
                    this.msg = "Mã định danh chưa tồn tại!";
                    break;
                case CodeStatusEnum.DA_TON_TAI:
                    this.msg = "Mã định danh đã tồn tại!";
                    this.block = data.Data;
                    this.data = data.DataExtra;
                    break;
                case CodeStatusEnum.DA_CAP_NHAT:
                    this.msg = "Mã định danh đã được cập nhật!";
                    this.data = data.Data;
                    break;
                default:
                    break;
            }
        }
    }
}
