import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { InvestmentRateRepository } from 'src/app/infrastructure/repositories/investment-rate.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-investment-rate',
    templateUrl: './add-or-update-investment-rate.component.html'
})

export class AddOrUpdateInvestmentRateComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;
    TypeReportApply_data = TypeReportApply;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    data_tableItemRef: any;
    invalid_tableItemRef = true;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.INVESTMENT_RATE_MANAGEMENT);
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'lineHeader', render: 'lineTpl' },
        { renderTitle: 'detailHeader', render: 'detailTpl' },
        { renderTitle: 'valueHeader', render: 'valueTpl', className: "text-right", width: 150 },
        { renderTitle: 'value1Header', render: 'value1Tpl', className: "text-right", width: 150 },
        { renderTitle: 'value2Header', render: 'value2Tpl', className: "text-right", width: 150 },
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
                    iif: i => !i.edit && this.role.Delete,
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

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private investmentRateRepository: InvestmentRateRepository, private cdr: ChangeDetectorRef, private message: NzMessageService, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
            TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, [Validators.required]],
            // Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            // Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, [Validators.required]],
            // Value: [this.record ? this.record.Value : undefined, [Validators.required]],
            // Value1: [this.record ? this.record.Value1 : undefined, [Validators.required]],
            // Value2: [this.record ? this.record.Value2 : undefined, [Validators.required]],
            investmentRateItems: [this.record ? this.record.investmentRateItems.map((item: any, index: number) => {
                item.index = index + 1;
                return item;
            }) : [], []]
        });

        this.data_tableItemRef = [...this.validateForm.value.investmentRateItems];
        this.invalid_tableItemRef = this.validateForm.value.Id ? false : true;
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.investmentRateItems = this.tableItemRef._data;

        const resp = data.Id ? await this.investmentRateRepository.update(data) : await this.investmentRateRepository.addNew(data);
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
            InvestmentRateId: undefined,
            LineInfo: undefined,
            DetailInfo: undefined,
            Value: undefined,
            Value1: undefined,
            Value2: undefined,
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
        if (!i['LineInfo'] || !i['DetailInfo'] || !i['Value'] || !i['Value1'] || !i['Value2']) {
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

        if (!i['LineInfo'] || !i['DetailInfo'] || !i['Value'] || !i['Value1'] || !i['Value2']) {
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
}
