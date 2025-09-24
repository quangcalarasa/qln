import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';

@Component({
    selector: 'app-add-or-update-lane',
    templateUrl: './add-or-update-lane.component.html'
})

export class AddOrUpdateLaneComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() isUpdateName: NzSafeAny;

    pdw_data = [];

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private laneRepository: LaneRepository, private cdr: ChangeDetectorRef, private provinceRepository: ProvinceRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, []],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Ward: [this.record ? this.record.Ward : undefined, [Validators.required]],
            District: [this.record ? this.record.District : undefined, [Validators.required]],
            Province: [this.record ? this.record.Province : undefined, [Validators.required]],
            Pdw: [this.record ? [this.record.Province, this.record.District, this.record.Ward] : undefined, [Validators.required]]
        });

        this.getCascaderData();
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? (this.isUpdateName == true ? await this.laneRepository.updateName(data) : await this.laneRepository.update(data)) : await this.laneRepository.addNew(data);
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

    async getCascaderData() {
        try {
            this.loading = true;
            const resp = await this.provinceRepository.getCascaderData(1);

            if (resp.meta?.error_code == 200) {
                this.pdw_data = resp.data;
            }
        } catch (error) {
            throw error;
        } finally {
            this.loading = false;
        }
    }

    changePdw() {
        let pdw = this.validateForm.value.Pdw;
        if (pdw.length == 0) {
            this.validateForm.value.Province = undefined;
            this.validateForm.value.District = undefined;
            this.validateForm.value.Ward = undefined;
        }
        else {
            this.validateForm.get('Province')?.setValue(pdw[0]);
            this.validateForm.get('District')?.setValue(pdw[1]);
            this.validateForm.get('Ward')?.setValue(pdw[2]);
        }

        this.cdr.detectChanges();
    }
}
