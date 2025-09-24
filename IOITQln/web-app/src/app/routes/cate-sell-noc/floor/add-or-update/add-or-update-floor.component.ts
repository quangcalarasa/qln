import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-floor',
    templateUrl: './add-or-update-floor.component.html'
})

export class AddOrUpdateFloorComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    role = this.commonService.CheckAccessKeyRole(AccessKey.FLOOR_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private floorRepository: FloorRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.floorRepository.update(data) : await this.floorRepository.addNew(data);
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
