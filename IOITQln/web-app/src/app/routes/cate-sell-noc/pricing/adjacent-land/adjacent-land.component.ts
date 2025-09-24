import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Decree, TermApply } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum, DecreeEnum } from 'src/app/shared/utils/enums';

@Component({
    selector: 'app-pricing-adjacent-land',
    templateUrl: './adjacent-land.component.html'
})

export class AdjacentLandComponent implements OnInit {
    @ViewChild('tableItemRef') public tableItemRef!: STComponent;
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() data: any[] = [];
    @Input() decreeMaps: any[] = [];

    termApply_data = TermApply;
    decree_data = Decree;

    TypeReportApplyEnum = TypeReportApplyEnum;
    DecreeEnum = DecreeEnum;

    data_tableItemRef: any;
    invalid_tableItemRef = true;
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'decreeHeader', render: 'decreeTpl' },
        { renderTitle: 'termApplyHeader', render: 'termApplyTpl' },
        { renderTitle: 'areaHeader', render: 'areaTpl', className: 'text-center' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit,
                    click: record => this.updateTableItemRefRow(record, true)
                },
                {
                    icon: 'delete',
                    iif: i => !i.edit,
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá bản ghi này?',
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

    constructor(private message: NzMessageService
    ) { }

    ngOnInit(): void {
        this.data_tableItemRef = [...this.data];
        this.invalid_tableItemRef = this.data.length == 0 ? true : false;

        this.decreeMaps.map((item: any) => {
            item.DecreeType1Id = item.DecreeType1Id ?? item.key;
            item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
            return item;
        });

        this.decreeMaps = this.decreeMaps.filter(x => x.DecreeType1Id != DecreeEnum.SPECIAL && x.DecreeType1Id != DecreeEnum.ND_61);
    }

    //Bảng con thêm cán bộ kỹ thuật
    addRow() {
        let row = {
            Id: undefined,
            DecreeType1Id: undefined,
            TermApply: undefined,
            PrivateArea: undefined,
            edit: true,
            index: this.data_tableItemRef.length + 1
        };

        this.tableItemRef.addRow(row);
        this.data_tableItemRef.push(Object.assign({}, row));
        this.checkTableItemRefIsValid();
    }

    checkTableItemRefIsValid() {
        if (this.tableItemRef._data.length == 0) this.invalid_tableItemRef = false;
        else {
            let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
            this.invalid_tableItemRef = isValid.length > 0 ? true : false;
        }

        this.eventEmitter.emit(this.invalid_tableItemRef);
    }

    private submit(i: STData): void {
        if (!i['DecreeType1Id'] || !i['TermApply'] || i['PrivateArea'] == undefined || i['PrivateArea']?.toString() == "") {
            this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
            this.checkTableItemRefIsValid();
        } else {
            this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
                if (item.index == i['index']) {
                    return Object.assign({}, i);
                } else return item;
            });

            this.updateTableItemRefRow(i, false);
            this.message.success(`Lưu bản ghi thành công!`);
        }
    }

    async deleteItem(i: STData) {
        this.tableItemRef.removeRow(i);
        this.message.create('success', `Xóa bản ghi!`);

        this.checkTableItemRefIsValid();
    }

    private updateTableItemRefRow(i: STData, edit: boolean): void {
        this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

        this.checkTableItemRefIsValid();
    }

    private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
        let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

        if (!i['DecreeType1Id'] || !i['TermApply'] || i['PrivateArea'] == undefined || i['PrivateArea']?.toString() == "") {
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
