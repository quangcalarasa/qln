import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PositionRepository } from 'src/app/infrastructure/repositories/position.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
    selector: 'app-add-or-update-position',
    templateUrl: './add-or-update-position.component.html'
})

export class AddOrUpdatePositionComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() departments: NzSafeAny;

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private positionRepository: PositionRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            DepartmentId: [this.record ? this.record.DepartmentId : undefined, []],
            Note: [this.record ? this.record.Note : undefined],
            AuthorDocsCode: [this.record ? this.record.AuthorDocsCode : undefined, []]
        });
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };

        const resp = data.Id ? await this.positionRepository.update(data) : await this.positionRepository.addNew(data);
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
