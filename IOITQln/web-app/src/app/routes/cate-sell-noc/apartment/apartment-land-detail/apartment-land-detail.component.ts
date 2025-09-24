import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TermApply, Decree } from 'src/app/shared/utils/consts';
import { TypeApartmentLandDetailEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';

@Component({
    selector: 'app-apartment-apartment-land-detail',
    templateUrl: './apartment-land-detail.component.html'
})

export class ApartmentLandDetailComponent implements OnInit {
    // @ViewChild('tableItemRef') private tableItemRef!: STComponent;
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() data: any[] = [];
    @Input() typeApartmentLandDetail: any;
    @Input() typeReportApply: number;
    @Input() parentTypeReportApply: number;

    typeApartmentLandDetailEnum = TypeApartmentLandDetailEnum;
    TypeReportApplyEnum = TypeReportApplyEnum;

    area_data: any[] = [];
    floor_data: any[] = [];

    // @Input() block: any;
    // @Input() apartmentDetails: any[] = [];

    // apartmentLandDetails: any = undefined;
    // invalid = true;

    // termApply_data = TermApply;

    // columnsItem: STColumn[] = [
    //     { title: 'Stt', type: 'no', width: 40 },
    //     { renderTitle: 'decreeTitle', render: 'decreeTpl' },
    //     { renderTitle: 'termApplyTitle', render: 'termApplyTpl' },
    //     { renderTitle: 'generalUseAreaTitle', render: 'generalUseAreaTpl', className: 'text-center' },
    //     { renderTitle: 'privateUseAreaTitle', render: 'personalUseAreaTpl', className: 'text-center' },
    //     {
    //         title: 'Chức năng',
    //         width: 100,
    //         className: 'text-center',
    //         buttons: [
    //             {
    //                 icon: 'edit',
    //                 iif: i => !i.edit,
    //                 click: record => this.updateRow(record, true)
    //             },
    //             {
    //                 icon: 'delete',
    //                 iif: i => !i.edit,
    //                 type: 'del',
    //                 pop: {
    //                     title: 'Bạn có chắc chắn muốn xoá chi tiết diện tích đất ở căn hộ này?',
    //                     okType: 'danger',
    //                     icon: 'star'
    //                 },
    //                 click: record => this.deleteRow(record)
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
    //                 click: record => this.cancelUpdateRow(record, false)
    //             }
    //         ]
    //     }
    // ];

    constructor(private message: NzMessageService,
        private areaRepository: AreaRepository,
        private floorRepository: FloorRepository
    ) { }

    ngOnInit(): void {
        // this.apartmentLandDetails = [...this.data];
        // this.invalid = this.data.length == 0 ? true : false;

        // this.block.decreeMaps.map((item: any) => {
        //     item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
        //     return item;
        // });
        if (this.typeApartmentLandDetail == TypeApartmentLandDetailEnum.APARTMENT && this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
            this.getDataArea();
            this.getDataFloor();
        }
    }

    // ngOnChanges(changes: SimpleChanges) {
    //     console.log("start");

    //     if (changes["apartmentDetails"] && this.apartmentLandDetails) {
    //         console.log("if else");

    //         if (this.apartmentDetails.length > 0) {
    //             let tableItemRefData: any[] = [];
    //             this.apartmentDetails.forEach(apartmentDetail => {
    //                 let apartmentLandDetail = this.apartmentLandDetails.find((x: any) => x.DecreeType1Id == apartmentDetail.DecreeType1Id && x.TermApply == apartmentDetail.TermApply);
    //                 if (apartmentLandDetail) {
    //                     let tableItemRefDataItem = tableItemRefData.find((x: any) => x.DecreeType1Id == apartmentDetail.DecreeType1Id && x.TermApply == apartmentDetail.TermApply);
    //                     if (!tableItemRefDataItem) {
    //                         let row = {
    //                             Id: undefined,
    //                             DecreeType1Id: apartmentDetail.DecreeType1Id,
    //                             TermApply: apartmentDetail.TermApply,
    //                             PrivateArea: 0,
    //                             GeneralArea: 0,
    //                             edit: true,
    //                             index: this.apartmentLandDetails.length + 1
    //                         };

    //                         tableItemRefData.push(row);
    //                     }
    //                     else {
    //                         tableItemRefData.push(tableItemRefDataItem);
    //                     }
    //                 }
    //             });


    //             this.apartmentLandDetails = [...tableItemRefData];
    //             this.tableItemRef._data = [...tableItemRefData];
    //         }
    //         else {
    //             this.apartmentLandDetails = [];
    //             this.tableItemRef._data = [];
    //         }
    //     }
    // }

    // addRow() {
    //     let row = {
    //         Id: undefined,
    //         DecreeType1Id: undefined,
    //         TermApply: undefined,
    //         PrivateArea: undefined,
    //         GeneralArea: undefined,
    //         edit: true,
    //         index: this.apartmentLandDetails.length + 1
    //     };

    //     this.tableItemRef.addRow(row, { index: row.index });
    //     this.apartmentLandDetails = [Object.assign({}, row)].concat(this.apartmentLandDetails);
    //     this.checkInValid();
    // }

    // checkInValid() {
    //     if (this.tableItemRef._data.length == 0) this.invalid = true;
    //     else {
    //         let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
    //         this.invalid = isValid.length > 0 ? true : false;
    //     }

    //     this.eventEmitter.emit(this.invalid);
    // }

    // private submit(i: STData): void {
    //     if (!i['DecreeType1Id'] || !i['TermApply'] || i['GeneralArea'] == undefined || i['PrivateArea'] == undefined) {
    //         this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
    //     } else {
    //         this.apartmentLandDetails = this.apartmentLandDetails.map((item: any) => {
    //             if (item.index == i['index']) {
    //                 i['edit'] = false;
    //                 return Object.assign({}, i);
    //             } else return item;
    //         });

    //         this.updateRow(i, false);
    //         this.message.success(`Lưu thông tin chi tiết diện tích đất ở căn hộ thành công!`);
    //     }

    //     this.checkInValid();
    // }

    // async deleteRow(i: STData) {
    //     this.tableItemRef.removeRow(i);
    //     this.apartmentLandDetails.splice(i, 1);
    //     this.message.create('success', `Xóa chi tiết diện tích đất ở căn hộ thành công!`);

    //     this.checkInValid();
    // }

    // private updateRow(i: STData, edit: boolean): void {
    //     this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });

    //     this.checkInValid();
    // }

    // private cancelUpdateRow(i: STData, edit: boolean): void {
    //     let item = this.apartmentLandDetails.find((x: any) => x.index == i['index']);

    //     if (!i['DecreeType1Id'] || !i['TermApply'] || i['GeneralArea'] == undefined || i['PrivateArea'] == undefined) {
    //         this.apartmentLandDetails = this.apartmentLandDetails.filter((x: any) => x != item);
    //         this.tableItemRef.removeRow(i);
    //     } else {
    //         item.edit = false;
    //         this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
    //     }

    //     this.checkInValid();
    // }

    // tableItemRefChange(e: STChange): void {
    //     switch (e.type) {
    //         case 'pi':
    //             break;
    //         case 'dblClick':
    //             break;
    //     }
    // }

    // getValue() {
    //     return this.tableItemRef._data.sort((a: any, b: any) => b - a);;
    // }

    emitChange() {
        this.eventEmitter.emit(this.data);
    }

    async getDataArea() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.query = `1=1`;
        paging.page_size = 0;
        paging.select = 'Id,Code,Name,FloorId,IsMezzanine';
        paging.order_by = 'Id Asc';

        const resp = await this.areaRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.area_data = resp.data;
        }
    }

    async getDataFloor() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.query = '1=1';
        paging.select = 'Id,Code,Name';
        paging.order_by = 'Id Asc';

        const resp = await this.floorRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.floor_data = resp.data;
        }
    }
}
