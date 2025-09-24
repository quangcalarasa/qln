import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167ReceiptRepository } from 'src/app/infrastructure/repositories/md167-receipt.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
    selector: 'app-md167-add-or-update-receipt',
    templateUrl: './add-or-update-receipt.component.html'
})

export class AddOrUpdateReceiptComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() contract: NzSafeAny;

    constructor(private modal: NzModalRef, private fb: FormBuilder,
        private md167ReceiptRepository: Md167ReceiptRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            ReceiptCode: [this.record ? this.record.ReceiptCode : undefined, [Validators.required]],
            DateOfPayment: [this.record ? convertDate(this.record.DateOfPayment) : undefined, [Validators.required]],
            DateOfReceipt: [this.record ? convertDate(this.record.DateOfReceipt) : undefined, [Validators.required]],
            Amount: [this.record ? this.record.Amount : undefined, [Validators.required]],
            PaidDeposit: [this.record ? this.record.PaidDeposit : undefined, []]
        });
    }
    
    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.Md167ContractId = this.contract.Id;

        const resp = data.Id ? await this.md167ReceiptRepository.update(data) : await this.md167ReceiptRepository.addNew(data);
        if (resp.meta?.error_code == 200) {
            this.loading = false;
            this.modal.triggerOk();
        }
        else {
            this.loading = false;
        }
    }

    close(): void {
        this.modal.close();
    }
}
