import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { UseValueCoefficientRepository } from 'src/app/infrastructure/repositories/use-value-coefficient.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-use-value-coefficient',
    templateUrl: './add-or-update-use-value-coefficient.component.html'
})

export class AddOrUpdateUseValueCoefficientComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;
    @Input() floor_data: NzSafeAny;

    TypeReportApply_data = TypeReportApply;

    data_tableItemRef: any;
    invalid_tableItemRef = true;
    role = this.commonService.CheckAccessKeyRole(AccessKey.USE_VALUE_COEFFICIENT_MANAGEMENT);
    decree_type1_data = Decree;

    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'floorHeader', render: 'floorTpl' },
        { title: 'Tầng lửng', render: 'isMezzanineTpl', className: "text-center", width: 150 },
        { renderTitle: 'valueHeader', render: 'valueTpl', className: "text-center", width: 150 },
        { renderTitle: 'noteHeader', render: 'noteTpl' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit && this.role.Update,
                    click: record => this.updateTableItemRefRow(record, true)
                },
                {
                    icon: 'delete',
                    iif: i => this.role.Delete,
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá hệ số điều chỉnh này?',
                        okType: 'danger',
                        icon: 'star'
                    },
                    click: record => this.deleteItem(record)
                },
                {
                    text: `Lưu`,
                    iif: i => i.edit,
                    type: 'link',
                    click: record => {
                        this.submit(record);
                    }
                },
                {
                    text: `Hủy`,
                    iif: i => i.edit,
                    click: record => this.cancelUpdateTableItemRefRow(record, false)
                }
            ]
        }
    ];

    constructor(private drawerRef: NzDrawerRef<string>, 
                private fb: FormBuilder, 
                private useValueCoefficientRepository: UseValueCoefficientRepository, 
                private cdr: ChangeDetectorRef, 
                private message: NzMessageService,
                private commonService: CommonService,
                ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            // TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, []],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, []],
            useValueCoefficientTypeReportApplies: [this.record ? (this.record.useValueCoefficientTypeReportApplies.length == 0 ? undefined : this.record.useValueCoefficientTypeReportApplies) : undefined, [Validators.required]],
            useValueCoefficientItems: [this.record ? this.record.useValueCoefficientItems.map((item: any, index: number) => {
                item.index = index + 1;
                return item;
            }) : [], []]
        });

        this.data_tableItemRef = [...this.validateForm.value.useValueCoefficientItems];
        this.invalid_tableItemRef = this.validateForm.value.Id ? false : true;
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.useValueCoefficientItems = this.tableItemRef._data;

        if (data.useValueCoefficientTypeReportApplies) {
            data.useValueCoefficientTypeReportApplies.forEach((x: any) => {
                x.TypeReportApply = x.TypeReportApply ?? x.key;

                return x;
            });
        }

        const resp = data.Id ? await this.useValueCoefficientRepository.update(data) : await this.useValueCoefficientRepository.addNew(data);
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

    //Bảng con
    addRow() {
        let row = {
            Id: undefined,
            UseValueCoefficientId: undefined,
            FloorId: undefined,
            IsMezzanine: undefined,
            Value: undefined,
            Note: undefined,
            edit: true,
            index: this.data_tableItemRef.length + 1
        };

        this.tableItemRef.addRow(row);
        this.data_tableItemRef.push(Object.assign({}, row));
        this.checkTableItemRefIsValid();
    }

    checkTableItemRefIsValid() {
        if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = true;
        else {
            let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
            this.invalid_tableItemRef = isValid.length > 0 ? true : false;
        }
    }

    private submit(i: STData): void {
        if (!i['FloorId'] || !i['Value'] || !i['Note']) {
            this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
        } else {
            this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
                if (item.index == i['index']) {
                    return Object.assign({}, i);
                } else return item;
            });

            this.updateTableItemRefRow(i, false);
            this.message.success(`Lưu hệ số điều chỉnh thành công!`);
        }

        this.checkTableItemRefIsValid();
    }

    async deleteItem(i: STData) {
        this.tableItemRef.removeRow(i);
        this.message.create('success', `Xóa hệ số điều chỉnh thành công!`);

        this.checkTableItemRefIsValid();
    }

    private updateTableItemRefRow(i: STData, edit: boolean): void {
        this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

        this.checkTableItemRefIsValid();
    }

    private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
        let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

        if (!item['FloorId'] || !item['Value'] || !item['Note']) {
            this.data_tableItemRef = this.data_tableItemRef.filter((x: any) => x != item);
            this.tableItemRef.removeRow(i);
        } else {
            item.edit = false;
            this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
        }

        this.checkTableItemRefIsValid();
    }

    tableItemRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                break;
            case 'dblClick':
                // this.openAddTypeAttributeItem(undefined);
                break;
        }
    }
    //End bảng con

    compareFn = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.key === o2.key || o1.TypeReportApply === parseInt(o2.key) : o1 === o2);
    };
}
