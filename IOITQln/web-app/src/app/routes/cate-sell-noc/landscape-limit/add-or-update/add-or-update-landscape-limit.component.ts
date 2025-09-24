import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { LandscapeLimitRepository } from 'src/app/infrastructure/repositories/landscape-limit.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { Decree } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-landscape-limit',
    templateUrl: './add-or-update-landscape-limit.component.html'
})

export class AddOrUpdateLandscapeLimitComponent implements OnInit {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;
    TypeReportApply_data = TypeReportApply;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;
    @Input() province_data: NzSafeAny;

    data_tableItemRef: any;
    invalid_tableItemRef = true;

    decree_type1_data = Decree;
    district_data: any[] = [];
    role = this.commonService.CheckAccessKeyRole(AccessKey.LANDSCAPE_LIMIT_MANAGEMENT);
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'districtHeader', render: 'districtTpl' },
        { renderTitle: 'limitAreaNormalHeader', render: 'limitAreaNormalTpl', className: "text-center", width: 150 },
        { renderTitle: 'limitAreaSpecialHeader', render: 'limitAreaSpecialTpl', className: "text-center", width: 150 },
        { renderTitle: 'inLimitPercentHeader', render: 'inLimitPercentTpl', className: "text-center", width: 150 },
        { renderTitle: 'outLimitPercentHeader', render: 'outLimitPercentTpl', className: "text-center", width: 150 },
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
                        title: 'Bạn có chắc chắn muốn xoá Chi tiết hạn mức đất ở này?',
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

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private landscapeLimitRepository: LandscapeLimitRepository, private cdr: ChangeDetectorRef, private message: NzMessageService, private districtRepository: DistrictRepository, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, [Validators.required]],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, []],
            TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, [Validators.required]],
            ProvinceId: [{ value: this.record ? this.record.ProvinceId : (this.province_data.find((x: any) => x.Code.trim() == "HCM")?.Id), disabled: true }, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, []],
            landscapeLimitItems: [this.record ? this.record.landscapeLimitItems.map((item: any, index: number) => {
                item.index = index + 1;
                return item;
            }) : [], []]
        });

        this.data_tableItemRef = [...this.validateForm.value.landscapeLimitItems];
        this.invalid_tableItemRef = this.validateForm.value.Id ? false : true;

        this.getDistrictData();
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.getRawValue() };
        data.landscapeLimitItems = this.tableItemRef._data;

        const resp = data.Id ? await this.landscapeLimitRepository.update(data) : await this.landscapeLimitRepository.addNew(data);
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
            LanscapeLimitId: undefined,
            DistrictId: undefined,
            LimitAreaNormal: undefined,
            LimitAreaSpecial: undefined,
            InLimitPercent: undefined,
            OutLimitPercent: undefined,
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
        if (!i['DistrictId'] || !i['LimitAreaNormal'] || !i['LimitAreaSpecial'] || !i['InLimitPercent'] || !i['OutLimitPercent']) {
            this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
        } else {
            this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
                if (item.index == i['index']) {
                    return Object.assign({}, i);
                } else return item;
            });

            this.updateTableItemRefRow(i, false);
            this.message.success(`Lưu Chi tiết hạn mức đất ở thành công!`);
        }

        this.checkTableItemRefIsValid();
    }

    async deleteItem(i: STData) {
        this.tableItemRef.removeRow(i);
        this.message.create('success', `Xóa Chi tiết hạn mức đất ở thành công!`);

        this.checkTableItemRefIsValid();
    }

    private updateTableItemRefRow(i: STData, edit: boolean): void {
        this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

        this.checkTableItemRefIsValid();
    }

    private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
        let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

        if (!i['DistrictId'] || !i['LimitAreaNormal'] || !i['LimitAreaSpecial'] || !i['InLimitPercent'] || !i['OutLimitPercent']) {
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

    //Danh sách quận huyện
    async getDistrictData() {
        let provinceId = this.validateForm.getRawValue().ProvinceId;

        let paging: GetByPageModel = new GetByPageModel();
        paging.query = `ProvinceId=${provinceId}`;
        paging.page_size = 0;
        paging.select = "Id,Name";

        const resp = await this.districtRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.district_data = resp.data;
        }
    }
}
