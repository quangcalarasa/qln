
import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';
import { NzModalService } from 'ng-zorro-antd/modal';
import { ExtraEmailDebtRepository } from 'src/app/infrastructure/repositories/extra-email-debt.repository';
import { ExtraConfigDebtRepository } from 'src/app/infrastructure/repositories/extra-debt-config.repository';

@Component({
    selector: 'app-config-debt',
    templateUrl: './config-debt.component.html',
    styles: [
    ]
})
export class ConfigDebtComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;
    dataBefore: any;
    record: any;

    constructor(private fb: FormBuilder,
        private modalSrv: NzModalService,
        private extraConfigDebtRepository: ExtraConfigDebtRepository ,

    ) {
        this.getData()

    }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Date: [undefined, [Validators.required]],
            DayOver: [undefined, [Validators.required]],
            Note: [undefined, [Validators.required]],
            Name: [undefined, [Validators.required]],
          });
    }
    async getData() {
        const resp = await this.extraConfigDebtRepository.getPort();

        if (resp.meta?.error_code == 200) {
            this.validateForm.get('Date')?.setValue(convertDate( resp.data.Date))
            this.validateForm.get('DayOver')?.setValue(resp.data.DayOver)
            this.validateForm.get('Note')?.setValue(resp.data.Note)
            this.validateForm.get('Name')?.setValue(resp.data.Name)
            this.record = resp.data
        } else {
            this.modalSrv.error({
                nzTitle: 'Không lấy được dữ liệu.'
            });
        }
    }
    clear() {
        this.validateForm = this.fb.group({
            Date: [this.record ? convertDate(this.record.Date) : undefined, [Validators.required]],
            DayOver: [this.record ? this.record.DayOver : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
        });
    }
    submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.getRawValue() };

        const resp = this.extraConfigDebtRepository.updatePort(data)
        this.loading = false;
        this.modalSrv.info({
            nzTitle: ' Cập nhật thành công'
        });
    }
}
