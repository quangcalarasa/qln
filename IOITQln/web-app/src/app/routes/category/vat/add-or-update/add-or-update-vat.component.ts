import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { VatRepository } from 'src/app/infrastructure/repositories/vat.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
    selector: 'app-add-or-update-vat',
    templateUrl: './add-or-update-vat.component.html'
})

export class AddOrUpdateVatComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;
    nzFormat = 'dd/ MM/ yyyy';
    @Input() record: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private vatRepository: VatRepository, private cdr: ChangeDetectorRef) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, [Validators.required]],
            Value: [this.record ? this.record.Value : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.vatRepository.update(data) : await this.vatRepository.addNew(data);
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
