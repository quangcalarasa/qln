import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ConstructionPriceRepository } from 'src/app/infrastructure/repositories/construction-price.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Decree } from 'src/app/shared/utils/consts';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-add-or-update-construction-price',
    templateUrl: './add-or-update-construction-price.component.html'
})

export class AddOrUpdateConstructionPriceComponent implements OnInit {
    // @ViewChild('tableItemRef') private tableItemRef!: STComponent;

    validateForm!: FormGroup;
    loading: boolean = false;

    @Input() record: NzSafeAny;
    @Input() decree_type2_data: NzSafeAny;
    @Input() unit_price_data: NzSafeAny;

    parent_data: any[] = [];

    // constructionPriceItems: any;
    // invalidConstructionPriceItem = true;

    decree_type1_data = Decree;
    role = this.commonService.CheckAccessKeyRole(AccessKey.CONSTRUCTION_PRICE_MANAGEMENT);
    // columnsItem: STColumn[] = [
    //     { title: 'Stt', type: 'no', width: 40 },
    //     { renderTitle: 'titleHeader', render: 'titleTpl' },
    //     { renderTitle: 'valueHeader', render: 'valueTpl', className: 'text-right' },
    //     {
    //         title: 'Chức năng',
    //         width: 100,
    //         className: 'text-center',
    //         buttons: [
    //             {
    //                 icon: 'edit',
    //                 iif: i => !i.edit,
    //                 click: record => this.updateConstructionPriceItem(record, true)
    //             },
    //             {
    //                 icon: 'delete',
    //                 iif: i => !i.edit,
    //                 type: 'del',
    //                 pop: {
    //                     title: 'Bạn có chắc chắn muốn xoá chỉ số giá này?',
    //                     okType: 'danger',
    //                     icon: 'star'
    //                 },
    //                 click: record => this.deleteItem(record)
    //             },
    //             {
    //                 text: `Lưu`,
    //                 iif: i => i.edit,
    //                 type: 'link',
    //                 click: record => {
    //                     this.submit(record);
    //                 }
    //             },
    //             {
    //                 text: `Hủy`,
    //                 iif: i => i.edit,
    //                 click: record => this.cancelUpdateConstructionPriceItem(record, false)
    //             }
    //         ]
    //     }
    // ];

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private constructionPriceRepository: ConstructionPriceRepository, private cdr: ChangeDetectorRef, private message: NzMessageService, private commonService: CommonService,) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            ParentId: [this.record ? this.record.ParentId : undefined, []],
            DecreeType1Id: [this.record ? this.record.DecreeType1Id.toString() : undefined, []],
            DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
            Des: [this.record ? this.record.Des : undefined, [Validators.required]],
            NameOfConstruction: [this.record ? this.record.NameOfConstruction : undefined, [Validators.required]],
            Note: [this.record ? this.record.Note : undefined, []],
            Year: [this.record ? this.record.Year : undefined, [Validators.required]],
            YearCompare: [this.record ? this.record.YearCompare : undefined, [Validators.required]],
            Value: [this.record ? this.record.Value : undefined, [Validators.required]]
            // constructionPriceItems: [this.record ? this.record.constructionPriceItems.map((item: any, index: number) => {
            //     item.index = index + 1;
            //     return item;
            // }) : [], []]
        });

        // this.constructionPriceItems = [...this.validateForm.value.constructionPriceItems];
        // this.invalidConstructionPriceItem = this.validateForm.value.Id ? false : true;

        // this.getDataParent();
    }


    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        // data.constructionPriceItems = this.tableItemRef._data;

        const resp = data.Id ? await this.constructionPriceRepository.update(data) : await this.constructionPriceRepository.addNew(data);
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

        const resp = await this.constructionPriceRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.parent_data = resp.data;
        }
    }

    // addRow() {
    //     let row = {
    //         Id: undefined,
    //         ConstructionPriceId: undefined,
    //         Title: undefined,
    //         Value: undefined,
    //         edit: true,
    //         index: this.constructionPriceItems.length + 1
    //     };

    //     this.tableItemRef.addRow(row);
    //     this.constructionPriceItems.push(Object.assign({}, row));
    //     this.checkConstructionPriceItemsIsValid();
    // }

    // checkConstructionPriceItemsIsValid() {
    //     if (this.tableItemRef._data.length == 0) this.invalidConstructionPriceItem = true;
    //     else {
    //         let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
    //         this.invalidConstructionPriceItem = isValid.length > 0 ? true : false;
    //     }
    // }

    // private submit(i: STData): void {
    //     if (!i['Title'] || !i['Value']) {
    //         this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    //     } else {
    //         this.constructionPriceItems = this.constructionPriceItems.map((item: any) => {
    //             if (item.index == i['index']) {
    //                 return Object.assign({}, i);
    //             } else return item;
    //         });

    //         this.updateConstructionPriceItem(i, false);
    //         this.message.success(`Lưu chỉ số giá thành công!`);
    //     }

    //     this.checkConstructionPriceItemsIsValid();
    // }

    // async deleteItem(i: STData) {
    //     this.tableItemRef.removeRow(i);
    //     this.message.create('success', `Xóa chỉ số giá thành công!`);

    //     this.checkConstructionPriceItemsIsValid();
    // }

    // private updateConstructionPriceItem(i: STData, edit: boolean): void {
    //     this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    //     this.checkConstructionPriceItemsIsValid();
    // }

    // private cancelUpdateConstructionPriceItem(i: STData, edit: boolean): void {
    //     let item = this.constructionPriceItems.find((x: any) => x.index == i['index']);

    //     if (!item['Title'] || !item['Value']) {
    //         this.constructionPriceItems = this.constructionPriceItems.filter((x: any) => x != item);
    //         this.tableItemRef.removeRow(i);
    //     } else {
    //         item.edit = false;
    //         this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    //     }

    //     this.checkConstructionPriceItemsIsValid();
    // }

    // tableItemRefChange(e: STChange): void {
    //     switch (e.type) {
    //         case 'pi':
    //             break;
    //         case 'dblClick':
    //             // this.openAddTypeAttributeItem(undefined);
    //             break;
    //     }
    // }
}
