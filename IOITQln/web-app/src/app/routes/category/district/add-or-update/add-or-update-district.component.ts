import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
    selector: 'app-add-or-update-district',
    templateUrl: './add-or-update-district.component.html'
})

export class AddOrUpdateDistrictComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() data_province: NzSafeAny;
    @Input() isUpdateName: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private districtRepository: DistrictRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            ProvinceId: [this.record ? this.record.ProvinceId : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ?(this.isUpdateName == true ? await this.districtRepository.updateName(data) : await this.districtRepository.update(data)) : await this.districtRepository.addNew(data);
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
