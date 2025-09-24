import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { CodeStatusEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';

@Component({
    selector: 'app-block-search-rent-block',
    templateUrl: './search-rent-block.component.html'
})
export class SearchRentBlockComponent implements OnInit {
    validateForm!: FormGroup;

    typeReportApply_data = TypeReportApply;

    data: any;

    codeStatusEnum = CodeStatusEnum;
    codeStatus?: CodeStatusEnum; //Trạng thái của mã định danh
    msg: string = "Mã định danh chưa tồn tại!";
    blockId?: number;
    TypeReportApplyEnum = TypeReportApplyEnum;

    constructor(
        private fb: FormBuilder,
        private modal: NzModalRef,
        private blockRepository: BlockRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Code: [undefined, [Validators.required]],
            TypeReportApply: [undefined, [Validators.required]],
        });
    }

    async submitForm() {
        this.data = { ...this.validateForm.value };

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

        const resp = await this.blockRepository.checkCodeStatus(data);

        if (resp.meta?.error_code == 200) {
            let data = resp.data;
            this.codeStatus = data.CodeStatus;
            this.blockId = data.Data?.Id;

            switch (this.codeStatus) {
                case CodeStatusEnum.CHUA_TON_TAI:
                    this.msg = "Mã định danh chưa tồn tại!"
                    break;
                case CodeStatusEnum.DA_TON_TAI:
                    this.msg = "Mã định danh đã tồn tại!"
                    this.validateForm.get('TypeReportApply')?.setValue(data.Data?.TypeReportApply?.toString());
                    break;
                case CodeStatusEnum.DA_CAP_NHAT:
                    this.msg = "Mã định danh đã được cập nhật!"
                    break;
                default:
                    break;
            }
        }
    }
}
