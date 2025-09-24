import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { VerifycationUnitRepository } from 'src/app/infrastructure/repositories/verifycation-unit.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
    selector: 'app-add-or-update-verifycation-unit',
    templateUrl: './add-or-update-verifycation-unit.component.html'
})

export class AddOrUpdateVerifycationUnitComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() doc_typies: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private verifycationUnitRepository: VerifycationUnitRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Address: [this.record ? this.record.Address : undefined],
            Note: [this.record ? this.record.Note : undefined],
            Type: [this.record ? this.record.Type : undefined, [Validators.required]],
            VerifyStatus: [this.record ? this.record.VerifyStatus : undefined]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.verifycationUnitRepository.update(data) : await this.verifycationUnitRepository.addNew(data);
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
