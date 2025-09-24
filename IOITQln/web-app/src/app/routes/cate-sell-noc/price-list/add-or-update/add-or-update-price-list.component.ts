import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PriceListRepository } from 'src/app/infrastructure/repositories/price-list.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-price-list',
    templateUrl: './add-or-update-price-list.component.html'
})

export class AddOrUpdatePricelistComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;
    @Input() unit_price_data: NzSafeAny;

    parent_data: any[] = [];

    data_tableItemRef: any;
    invalid_tableItemRef = true;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.PRICELIST_MANAGEMENT);
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'nameHeader', render: 'nameTpl' },
        { renderTitle: 'detailStructureHeader', render: 'detailStructureTpl' },
        { renderTitle: 'valueTypePile1Header', render: 'value1Tpl', className: "text-right" },
        { renderTitle: 'valueTypePile2Header', render: 'value2Tpl', className: "text-right" },
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
                        title: 'Bạn có chắc chắn muốn xoá chi tiết giá nhà ở này?',
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
                private priceListRepository: PriceListRepository, 
                private cdr: ChangeDetectorRef, 
                private message: NzMessageService,
                private commonService: CommonService,
                ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            // ParentId: [this.record ? this.record.ParentId : undefined, []],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, []],
            priceListItems: [this.record ? this.record.priceListItems.map((item: any, index: number) => {
                item.index = index + 1;
                return item;
            }) : [], []]
            // NameOfConstruction: [this.record ? this.record.NameOfConstruction : undefined, [Validators.required]],
            // IsMezzanine: [this.record ? this.record.IsMezzanine : undefined, []],
            // Note: [this.record ? this.record.Note : undefined, []],
            // UnitPriceId: [this.record ? this.record.UnitPriceId : undefined, []],
            // ValueTypePile1: [this.record ? this.record.ValueTypePile1 : undefined, [Validators.required]],
            // ValueTypePile2: [this.record ? this.record.ValueTypePile2 : undefined, [Validators.required]]
        });

        // this.getDataParent();
        this.data_tableItemRef = [...this.validateForm.value.priceListItems];
        this.invalid_tableItemRef = this.validateForm.value.Id ? false : true;
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.priceListItems = this.tableItemRef._data;

        const resp = data.Id ? await this.priceListRepository.update(data) : await this.priceListRepository.addNew(data);
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

    async getDataParent() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = this.record ? `ParentId=null AND Id!=${this.record.Id}` : 'ParentId=null';
        paging.select = "Id,NameOfConstruction";

        const resp = await this.priceListRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.parent_data = resp.data;
        }
    }

    //Bảng con
    addRow() {
        let row = {
            Id: undefined,
            PriceListId: undefined,
            NameOfConstruction: undefined,
            ValueTypePile1: undefined,
            ValueTypePile2: undefined,
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
        if (!i['NameOfConstruction'] || i['ValueTypePile1'] == undefined || i['ValueTypePile2'] == undefined || !i['DetailStructure']) {
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

        if (!item['NameOfConstruction'] || item['ValueTypePile1'] == undefined || item['ValueTypePile2'] == undefined || !i['DetailStructure']) {
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
