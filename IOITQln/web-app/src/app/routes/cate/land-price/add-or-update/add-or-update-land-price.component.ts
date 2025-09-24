import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LandPriceRepository } from 'src/app/infrastructure/repositories/land-price.repository';
import { UnitPriceRepository } from 'src/app/infrastructure/repositories/unit-price.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { ProvinceRepository } from 'src/app/infrastructure/repositories/province.repository';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey, LandPriceType } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-land-price',
    templateUrl: './add-or-update-land-price.component.html'
})

export class AddOrUpdateLandPriceComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    // @Input() decree_type1_data: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;

    unit_data: any[] = [];
    lane_date: any[] = [];
    pd_data = [];

    data_tableItemRef: any;
    invalid_tableItemRef = true;
    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.LAND_PRICE_MANAGEMENT);

    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'laneNameHeader', render: 'laneNameTpl' },
        { renderTitle: 'laneNameStartHeader', render: 'laneNameStartTpl' },
        { title: 'Đoạn đường đến', render: 'laneNameEndTpl' },
        { renderTitle: 'valueHeader', render: 'valueTpl', className: "text-right" },
        { renderTitle: 'descHeader', render: 'desTpl' },
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
                        title: 'Bạn có chắc chắn muốn xoá chi tiết giá đất này?',
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
                private landPriceRepository: LandPriceRepository, 
                private cdr: ChangeDetectorRef, 
                private unitPriceRepository: UnitPriceRepository, 
                private laneRepository: LaneRepository, 
                private provinceRepository: ProvinceRepository, 
                private message: NzMessageService,
                private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            // LaneId: [this.record ? this.record.LaneId : undefined, [Validators.required]],
            // LaneStartId: [this.record ? this.record.LaneStartId : undefined, [Validators.required]],
            MinValue: [this.record ? this.record.MinValue : undefined, []],
            Province: [this.record ? this.record.Province : undefined, [Validators.required]],
            District: [this.record ? this.record.District : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, [Validators.required]],
            Pd: [this.record ? [this.record.Province, this.record.District] : undefined, [Validators.required]],
            landPriceItems: [this.record ? this.record.landPriceItems.map((item: any, index: number) => {
                item.index = index + 1;
                return item;
            }) : [], []],
            LandPriceType: [this.record ? this.record.LandPriceType : LandPriceType.NOC, [Validators.required]]
        });

        this.data_tableItemRef = [...this.validateForm.value.landPriceItems];
        this.invalid_tableItemRef = this.validateForm.value.Id ? false : true;

        this.getDataBlock();
        // this.getDataLane();
        this.getCascaderData();
    }

    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.landPriceItems = this.tableItemRef._data;

        const resp = data.Id ? await this.landPriceRepository.update(data) : await this.landPriceRepository.addNew(data);
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

    async getDataBlock() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.query = `1=1`;
        paging.page_size = 0;
        paging.select = "Id,Code,Name";

        const resp = await this.unitPriceRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.unit_data = resp.data;
        }
    }

    async getDataLane() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.query = `1=1`;
        paging.page_size = 0;
        paging.select = "Id,Name";

        const resp = await this.laneRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.lane_date = resp.data;
        }
    }

    async getCascaderData() {
        try {
            this.loading = true;
            const resp = await this.provinceRepository.getCascaderData(2);

            if (resp.meta?.error_code == 200) {
                this.pd_data = resp.data;
            }
        } catch (error) {
            throw error;
        } finally {
            this.loading = false;
        }
    }

    changePd() {
        let pd = this.validateForm.value.Pd;
        if (pd.length == 0) {
            this.validateForm.value.Province = undefined;
            this.validateForm.value.District = undefined;
            // this.validateForm.value.Ward = undefined;
            // this.getLaneData(undefined);
        }
        else {
            this.validateForm.get('Province')?.setValue(pd[0]);
            this.validateForm.get('District')?.setValue(pd[1]);
            // this.validateForm.get('Ward')?.setValue(pdw[2]);

            // this.getLaneData(pdw[2]);
        }

        this.cdr.detectChanges();
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
        if (!i['LaneName'] || !i['LaneStartName'] || !i['Value'] || !i['Des']) {
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

        if (!item['LaneName'] || !item['LaneStartName'] || !item['Value'] || !item['Des']) {
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
