import { Component, Input, OnInit, Output, EventEmitter, ViewChild, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { NzMessageService } from 'ng-zorro-antd/message';
import { LevelBlock, TermApply, Decree } from 'src/app/shared/utils/consts';
import { CommonService } from 'src/app/core/services/common.service';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { DistributionFloorCoefficientRepository } from 'src/app/infrastructure/repositories/distribution-floor-coefficient.repository';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { FloorRepository } from 'src/app/infrastructure/repositories/floor.repository';
import { UseValueCoefficientRepository } from 'src/app/infrastructure/repositories/use-value-coefficient.repository';
import { TypeApartmentDetailEnum, TypeReportApplyEnum, DecreeEnum, TermApplyEnum } from 'src/app/shared/utils/enums';
import { FilterTermApplyByDecreePipe } from 'src/app/shared/pipe/filter-termapply-by-decree.pipe';
import { KeyValuePipe } from "@angular/common";

@Component({
    selector: 'app-apartment-apartment-detail',
    templateUrl: './apartment-detail.component.html'
})

export class ApartmentDetailComponent implements OnInit, OnChanges {
    @ViewChild('tableItemRef') private tableItemRef!: STComponent;
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() data: any[] = [];
    @Input() typeApartmentDetail: any;

    @Input() decreeMaps: any[] = [];
    @Input() typeReportApply: number;
    @Input() floorApplyPriceChange: number;
    @Input() specialCase: boolean;
    @Input() parentTypeReportApply: number;
    @Input() levelBlockMaps: any[] = [];

    apartmentDetails: any;
    invalidApartmentDetail = false;

    level_data = LevelBlock;
    termApply_data = TermApply;

    distribution_floor_coefficient_data = [];
    area_data: any[] = [];
    floor_data: any[] = [];
    usevaluecoefficientitem_data: any[] = [];

    TypeReportApplyEnum = TypeReportApplyEnum;
    DecreeEnum = DecreeEnum;
    TermApplyEnum = TermApplyEnum;
    
    columnsItem: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { renderTitle: 'decreeTitle', render: 'decreeTpl' },
        { renderTitle: 'termApplyTitle', render: 'termApplyTpl' },
        { renderTitle: 'levelTitle', render: 'levelTpl' },
        { renderTitle: 'floorTitle', render: 'floorTpl' },
        { renderTitle: 'areaTitle', render: 'areaTpl' },
        { renderTitle: 'floorApplyCoefficientTitle', render: 'floorApplyCoefficientTpl', iif: () => this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU },
        { renderTitle: 'generalAreaTitle', render: 'generalAreaValueTpl', className: 'text-center', iif: () => this.typeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU && this.typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.typeReportApply != TypeReportApplyEnum.NHA_RIENG_LE },
        { renderTitle: 'privateAreaTitle', render: 'personalAreaValueTpl', className: 'text-center' },
        { renderTitle: 'coefficientDTitle', render: 'coefficientDTpl', className: 'text-center', iif: () => ((this.typeApartmentDetail == TypeApartmentDetailEnum.APARTMENT && this.typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) || this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) },
        { renderTitle: 'coefficientUvTitle', render: 'coefficientUvTpl', className: 'text-center', iif: () => this.typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG },
        { renderTitle: 'applyInvestmentRateTitle', render: 'applyInvestmentRateTpl', iif: () => this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU || (this.typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.parentTypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) },
        {
            title: 'Chức năng',
            width: 100,
            className: this.invalidApartmentDetail ? 'text-center disabled-btn-custom' : 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit,
                    click: record => this.updateApartmentDetail(record, true)
                },
                {
                    icon: 'delete',
                    iif: i => !i.edit,
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá chi tiết căn hộ này?',
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
                    },
                },
                {
                    text: `Hủy`,
                    iif: i => i.edit,
                    click: record => this.cancelUpdateApartmentDetail(record, false)
                }
            ]
        }
    ];

    constructor(private message: NzMessageService, private commonService: CommonService, private distributionFloorCoefficientRepository: DistributionFloorCoefficientRepository, private areaRepository: AreaRepository,
        private floorRepository: FloorRepository,
        private useValueCoefficientRepository: UseValueCoefficientRepository,
        private cdr: ChangeDetectorRef,
        private keyValuePipe: KeyValuePipe,
        private filterTermApplyByDecreePipe: FilterTermApplyByDecreePipe
    ) { }

    ngOnInit(): void {
        this.apartmentDetails = [...this.data];
        this.invalidApartmentDetail = this.data.length == 0 ? true : false;

        this.decreeMaps.map((item: any) => {
            item.DecreeType1Id = item.DecreeType1Id ?? item.key;
            item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
            return item;
        });

        this.getDistributionFloorCoefficient(this.decreeMaps);
        this.getDataFloor();
        this.getDataArea();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes["typeReportApply"]) {
            this.getUseValueCoefficientItemData();
        }

        if (changes["decreeMaps"]) {
            this.decreeMaps.map((item: any) => {
                item.DecreeType1Id = item.DecreeType1Id ?? item.key;
                item.DecreeType1Name = Decree[item.DecreeType1Id as unknown as keyof typeof Decree];
                return item;
            });
        }

        if (changes["floorApplyPriceChange"]) { }
        if (changes["specialCase"]) { }

    }

    async getUseValueCoefficientItemData() {
        this.usevaluecoefficientitem_data = [];

        if (this.typeReportApply) {
            const resp = await this.useValueCoefficientRepository.getUseValueCoefficientItems(this.typeReportApply);

            if (resp.meta?.error_code == 200) {
                this.usevaluecoefficientitem_data = resp.data;
            }
        }
    }

    addRow() {
        let row = {
            Id: undefined,
            DecreeType1Id: this.getDefaultDecreeType1Id(),
            TermApply: this.getDefaultTermApply(),
            Level: this.getDefaultLevel(),
            FloorApplyCoefficient: this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU ? undefined : this.floorApplyPriceChange,
            AreaId: undefined,
            FloorId: undefined,
            PrivateArea: undefined,
            GeneralArea: undefined,
            CoefficientDistribution: undefined,
            CoefficientUseValue: undefined,
            FloorApplyPriceChange: this.floorApplyPriceChange,
            ApplyInvestmentRate: true,
            IsMezzanine: false,
            edit: true,
            index: this.apartmentDetails.length + 1
        };

        // this.tableItemRef.addRow(row);
        this.tableItemRef.addRow(row, { index: row.index });
        // this.apartmentDetails.push(Object.assign({}, row));
        this.apartmentDetails = [Object.assign({}, row)].concat(this.apartmentDetails);
        this.checkApartmentDetailsIsValid();
    }

    getDefaultDecreeType1Id() {
        return this.decreeMaps.length == 1 ? this.decreeMaps[0].DecreeType1Id : undefined
    }

    getDefaultTermApply() {
        let decreeType1Id = this.getDefaultDecreeType1Id();

        let termApply_data = this.filterTermApplyByDecreePipe.transform(this.keyValuePipe.transform(this.termApply_data), decreeType1Id, this.typeReportApply);

        if (termApply_data) {
            if (termApply_data.length == 1) {
                return termApply_data[0].key;
            }
            else return undefined;
        }
        else {
            return undefined;
        }
    }

    getDefaultLevel() {
        if (this.levelBlockMaps.length == 1) {
            let levelBlockMap = this.levelBlockMaps[0];
            return levelBlockMap.LevelId ? levelBlockMap.LevelId.toString() : levelBlockMap.key;
        }
        else {
            return undefined;
        }
    }

    checkApartmentDetailsIsValid() {
        if (this.tableItemRef._data.length == 0) this.invalidApartmentDetail = false;
        else {
            let isValid = this.tableItemRef._data.filter(x => x['edit'] == true);
            this.invalidApartmentDetail = isValid.length > 0 ? true : false;
        }

        // this.changeLevelBlockMaps();
        this.cdr.detectChanges();
        this.eventEmitter.emit(this.invalidApartmentDetail);
    }

    private submit(i: STData): void {
        if (!i['DecreeType1Id'] || !i['TermApply'] || !i['Level'] || !i['FloorId'] || !i['AreaId'] || i['PrivateArea'] == undefined || i['PrivateArea']?.toString() == "" || ((i['GeneralArea'] == undefined || i['GeneralArea']?.toString() == "") && this.typeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU && this.typeReportApply != TypeReportApplyEnum.NHA_RIENG_LE && this.typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) || (!i['FloorApplyPriceChange'] && this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU)) {
            this.tableItemRef.setRow(i, { submit: true }, { refreshSchema: true });
        } else {
            // Tính H.s giá trị sử dụng
            let area = this.area_data.find(x => x.Id == i['AreaId']);
            if (area != null) {
                let floorId = i['FloorId'];
                area.IsMezzanine = area.IsMezzanine ?? false;
                // i['IsMezzanine'] = area.IsMezzanine;
                // i['ApplyInvestmentRate'] = i['DecreeType1Id'] == DecreeEnum.ND_99 && area.IsMezzanine != true ? true : false;
                let usevaluecoefficientitem = this.usevaluecoefficientitem_data.find(
                    x => x.FloorId == floorId && (x.IsMezzanine ?? false) == area.IsMezzanine && x.DecreeType1Id == i['DecreeType1Id']
                );

                if (usevaluecoefficientitem) i['CoefficientUseValue'] = usevaluecoefficientitem.Value;
            }

            let floor = this.floor_data.find(x => x.Id == i['FloorId']);

            //Tính hệ số phân bổ đất
            if (this.typeApartmentDetail == TypeApartmentDetailEnum.APARTMENT) i['CoefficientDistribution'] = this.commonService.getCoefficient(floor.Code, (this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU ? i['FloorApplyPriceChange'] : this.floorApplyPriceChange), area.IsMezzanine, i['DecreeType1Id'], this.distribution_floor_coefficient_data, this.specialCase);

            this.apartmentDetails = this.apartmentDetails.map((item: any) => {
                if (item.index == i['index']) {
                    i['edit'] = false;
                    return Object.assign({}, i);
                } else return item;
            });

            // this.apartmentDetails = this.apartmentDetails.sort((a: any, b: any) => a.Level - b.Level);

            this.updateApartmentDetail(i, false);
            this.message.success(`Lưu thông tin chi tiết căn hộ thành công!`);
        }

        this.checkApartmentDetailsIsValid();
    }

    async deleteItem(i: STData) {
        this.tableItemRef.removeRow(i);
        this.apartmentDetails.splice(i, 1);
        this.message.create('success', `Xóa chi tiết căn hộ thành công!`);

        this.checkApartmentDetailsIsValid();
    }

    private updateApartmentDetail(i: STData, edit: boolean): void {
        this.tableItemRef.setRow(i, { edit }, { refreshSchema: true });
        // this.tableItemRef._data = this.tableItemRef._data.sort((a: any, b: any) => a.Level - b.Level);

        this.checkApartmentDetailsIsValid();
    }

    private cancelUpdateApartmentDetail(i: STData, edit: boolean): void {
        let item = this.apartmentDetails.find((x: any) => x.index == i['index']);

        if (!i['DecreeType1Id'] || !i['TermApply'] || !i['Level'] || !i['FloorId'] || !i['AreaId'] || i['PrivateArea'] == undefined || i['PrivateArea']?.toString() == "" || ((i['GeneralArea'] == undefined || i['GeneralArea']?.toString() == "") && this.typeReportApply != TypeReportApplyEnum.NHA_CHUNG_CU && this.typeReportApply != TypeReportApplyEnum.NHA_RIENG_LE && this.typeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) || (!i['FloorApplyPriceChange'] && this.typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU)) {
            this.apartmentDetails = this.apartmentDetails.filter((x: any) => x != item);
            this.tableItemRef.removeRow(i);
        } else {
            item.edit = false;
            this.tableItemRef.setRow(i, Object.assign({}, item), { refreshSchema: true });
        }

        this.checkApartmentDetailsIsValid();
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

    getValue() {
        return this.tableItemRef._data.sort((a: any, b: any) => b - a);
    }

    //lấy ds hệ số phân bổ các tầng theo nghị định
    async getDistributionFloorCoefficient(decreeMaps: any) {
        this.distribution_floor_coefficient_data = [];
        if (decreeMaps.length) {
            let paging: GetByPageModel = new GetByPageModel();
            paging.page_size = 0;
            // paging.query = `DecreeType1Id=${DecreeType1Id}`;
            paging.query = decreeMaps.map((d: any) => "DecreeType1Id=" + (d.key ?? d.DecreeType1Id)).join(' OR ');

            const resp = await this.distributionFloorCoefficientRepository.getByPage(paging);

            if (resp.meta?.error_code == 200) {
                this.distribution_floor_coefficient_data = resp.data;
            }
        }
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

    getIsMezzanine(item: any): boolean {
        let area = this.area_data.find(x => x.Id == item['AreaId']);

        return area ? (area.IsMezzanine ?? false) : true;
    }
}
