import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HolidayRepository } from 'src/app/infrastructure/repositories/holiday.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';

@Component({
    selector: 'app-add-or-update-holiday',
    templateUrl: './add-or-update-holiday.component.html'
})

export class AddOrUpdateHolidayComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private holidayRepository: HolidayRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            StartDate: [this.record ? convertDate(this.record.StartDate) : undefined, [Validators.required]],
            EndDate: [this.record ? convertDate(this.record.EndDate) : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.holidayRepository.update(data) : await this.holidayRepository.addNew(data);
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
