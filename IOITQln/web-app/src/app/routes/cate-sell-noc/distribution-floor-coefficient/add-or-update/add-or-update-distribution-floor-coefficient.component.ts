import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DistributionFloorCoefficientRepository } from 'src/app/infrastructure/repositories/distribution-floor-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NumberFloor } from 'src/app/shared/utils/consts';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-distribution-floor-coefficient',
    templateUrl: './add-or-update-distribution-floor-coefficient.component.html'
})

export class AddOrUpdateDistributionFloorCoefficientComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    number_floor_data = NumberFloor;
    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.DISTRIBUTION_FLOOR_COEFFICIENT_MANAGEMENT);
    columnsItem: STColumn[] = [
        { title: '', render: 'floorTpl' },
        { title: 'Tầng 1', render: 'value1Tpl', className: "text-center" },
        { title: 'Tầng 2', render: 'value2Tpl', className: "text-center" },
        { title: 'Tầng 3', render: 'value3Tpl', className: "text-center" },
        { title: 'Tầng 4', render: 'value4Tpl', className: "text-center" },
        { title: 'Tầng 5', render: 'value5Tpl', className: "text-center" },
        { title: 'Tầng 6 trở lên', render: 'value6Tpl', className: "text-center" }
    ];

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private distributionFloorCoefficientRepository: DistributionFloorCoefficientRepository, private cdr: ChangeDetectorRef, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            ApplyMezzanineCoefficient: [this.record ? this.record.ApplyMezzanineCoefficient : undefined, []],
            MezzanineCoefficient: [this.record ? this.record.MezzanineCoefficient : undefined, []],
            FlatCoefficient: [this.record ? this.record.FlatCoefficient : undefined, []],
            distributionFloorCoefficientDetails: [this.record ? this.record.distributionFloorCoefficientDetails.map((item: any, index: number) => {
                item.NumberFloor = item.NumberFloor.toString();
                return item;
            }) : this.initDistributionFloorCoefficientDetails(), []]
        });

        this.initDistributionFloorCoefficientDetails();
    }

    initDistributionFloorCoefficientDetails(): any[] {
        let res: any[] = [];
        Object.keys(NumberFloor).forEach(item => {
            res.push({
                DistributionFloorCoefficientId: undefined,
                NumberFloor: item,
                Value1: undefined,
                Value2: undefined,
                Value3: undefined,
                Value4: undefined,
                Value5: undefined,
                Value6: undefined
            });
        });

        return res;
    }

    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.distributionFloorCoefficientDetails = this.tableItemRef._data;

        const resp = data.Id ? await this.distributionFloorCoefficientRepository.update(data) : await this.distributionFloorCoefficientRepository.addNew(data);
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

    tableItemRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                break;
            case 'dblClick':
                break;
        }
    }
}
