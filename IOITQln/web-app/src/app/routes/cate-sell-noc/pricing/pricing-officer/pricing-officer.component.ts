import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
    selector: 'app-pricing-pricing-officer',
    templateUrl: './pricing-officer.component.html'
})

export class PricingOfficerComponent implements OnInit {
    @ViewChild('tableItemRef') public tableItemRef!: STComponent;
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() data: any[] = [];

    data_tableItemRef: any;
    invalid_tableItemRef = true;
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'nameHeader', render: 'nameTpl' },
        { renderTitle: 'functionHeader', render: 'functionTpl' },
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
                        title: 'Bạn có chắc chắn muốn xoá cán bộ kỹ thuật này?',
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
    }

    //Bảng con thêm cán bộ kỹ thuật
    addRow() {
        let row = {
            Id: undefined,
            Name: undefined,
            Function: undefined,
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
        if (!i['Name'] || !i['Function']) {
            this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
        } else {
            this.data_tableItemRef = this.data_tableItemRef.map((item: any) => {
                if (item.index == i['index']) {
                    return Object.assign({}, i);
                } else return item;
            });

            this.updateTableItemRefRow(i, false);
            this.message.success(`Lưu cán bộ kỹ thuật thành công!`);
        }

        this.checkTableItemRefIsValid();
    }

    async deleteItem(i: STData) {
        this.tableItemRef.removeRow(i);
        this.message.create('success', `Xóa cán bộ kỹ thuật thành công!`);

        this.checkTableItemRefIsValid();
    }

    private updateTableItemRefRow(i: STData, edit: boolean): void {
        this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

        this.checkTableItemRefIsValid();
    }

    private cancelUpdateTableItemRefRow(i: STData, edit: boolean): void {
        let item = this.data_tableItemRef.find((x: any) => x.index == i['index']);

        if (!item['Name'] || !item['Function']) {
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
