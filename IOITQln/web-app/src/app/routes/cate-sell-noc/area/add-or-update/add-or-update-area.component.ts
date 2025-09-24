import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-add-or-update-area',
    templateUrl: './add-or-update-area.component.html'
})

export class AddOrUpdateAreaComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() data_floor: NzSafeAny;
    role = this.commonService.CheckAccessKeyRole(AccessKey.AREA_MANAGEMENT);
    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private areaRepository: AreaRepository, private cdr: ChangeDetectorRef, private blockRepository: BlockRepository, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            FloorId: [this.record ? this.record.FloorId : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            IsMezzanine: [this.record ? this.record.IsMezzanine : undefined, []],
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.areaRepository.update(data) : await this.areaRepository.addNew(data);
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
