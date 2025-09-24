import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';
import { AddOrUpdateMd167ContractComponent } from '../add-or-update/add-or-update-contract.component';
import { ContractStatus167, Contract167Type } from 'src/app/shared/utils/consts';
import { Contract167TypeEnum, ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { AddOrUpdateMd167ContractExtraComponent } from '../add-or-update-extra/add-or-update-contract-extra.component';
import { Md167ContractDebtTableComponent } from '../debt-table/debt-table.component';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { STColumnButtonPop } from '@delon/abc/st';

@Component({
    selector: 'app-md167-list-contract',
    templateUrl: './list-contract.component.html'
})
export class ListContractComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    house_data: any[] = [];

    columns: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Số hợp đồng', index: 'Code', width: 180 },
        { title: 'Ngày ký hợp đồng', index: 'DateSign', type: 'date', className: 'text-center', width: 100, dateFormat: 'dd/MM/yyyy' },
        { title: 'Tên đơn vị thuê', index: 'DelegateName', width: 200 },
        { title: 'Hợp đồng hay phụ lục hợp đồng', width: 120, index: 'Type', type: 'enum', enum: Contract167Type },
        { title: 'Trạng thái hợp đồng', index: 'ContractStatus', type: 'enum', enum: ContractStatus167, width: 220 },
        { title: 'Mã nhà', index: 'HouseCode', width: 180 },
        { title: 'Số nhà', index: 'HouseNumber', width: 200 },
        { title: 'Đường', index: 'Lane', width: 250 },
        { title: 'Phường/xã/thị trấn', index: 'Ward', width: 150 },
        { title: 'Tp(trực thuộc)/quận/huyện', index: 'District', width: 150 },
        { title: 'Tp/tỉnh', index: 'Province', width: 150 },
        {
            title: 'Chức năng',
            width: 120,
            className: 'text-center',
            fixed: 'right',
            buttons: [
                {
                    icon: 'table',
                    iif: i => !i.edit,
                    click: record => this.debt(record),
                    tooltip: 'Công nợ'
                },
                {
                    icon: 'edit',
                    iif: i => !i.edit,
                    click: record => this.addOrUpdate(record),
                    tooltip: "Sửa"
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá biên bản tính giá này?',
                        okType: 'danger',
                        icon: 'star'
                    },
                    click: record => this.delete(record),
                    tooltip: "Xóa"
                }
            ]
        }
    ];

    ImportHistoryTypeEnum = ImportHistoryTypeEnum;

    constructor(
        private modalSrv: NzModalService,
        private md167ContractRepository: Md167ContractRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService
    ) { }

    ngOnInit(): void {
        this.getData();
        this.getHouseData();
    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                this.getData();
                break;
            case 'dblClick':
                break;
            case 'checkbox':
                break;
            case 'sort':
                this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
                this.getData();
                break;
            default:
                break;
        }
    }

    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
        this.getData();
    }

    searchData() {
        this.paging.page = 1;
        this.getData();
    }

    async getData() {
        this.paging.query = '1=1';
        this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Name.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and HouseId=${this.query.type}`
        }

        // if (this.query.dateStart && this.query.dateStart != "") {
        //     let splDateStart = this.query.dateStart.split("-");
        //     this.paging.query += `and CreatedAt >= DateTime(${splDateStart[0]},${splDateStart[1]},${splDateStart[2]},0,0,0)`;
        // }

        // if (this.query.dateEnd && this.query.dateEnd != "") {
        //     let splDateEnd = this.query.dateEnd.split("-");
        //     this.paging.query += `and CreatedAt <= DateTime(${splDateEnd[0]},${splDateEnd[1]},${splDateEnd[2]},23,59,59)`;
        // }

        try {
            this.loading = true;
            const resp = await this.md167ContractRepository.getByPage(this.paging);

            if (resp.meta?.error_code == 200) {
                this.data = resp.data;
                this.paging.item_count = resp.metadata;
            } else {
                this.modalSrv.error({
                    nzTitle: 'Không lấy được dữ liệu.'
                });
            }
        } catch (error) {
            throw error;
        } finally {
            this.loading = false;
        }
    }

    onBack() {
        window.history.back();
    }

    addOrUpdate(record?: any): void {
        if (record?.Type == Contract167TypeEnum.EXTRA) {
            this.modalSrv.create({
                nzTitle: `Sửa thông tin phụ lục cho hợp đồng "${record.Code}"`,
                nzContent: AddOrUpdateMd167ContractExtraComponent,
                nzWidth: '85vw',
                nzComponentParams: {
                    record: record,
                    parent: record.parent
                },
                nzOnOk: (res: any) => {
                    this.getData();
                    this.message.create('success', `Sửa phụ lục cho hợp đồng thành công!`);
                }
            });
        }
        else {
            const drawerRef = this.drawerService.create<AddOrUpdateMd167ContractComponent>({
                nzTitle: record ? `Sửa thông tin hợp đồng ${record.Code}` : 'Thêm mới hợp đồng',
                nzWidth: '85vw',
                nzPlacement: 'left',
                nzContent: AddOrUpdateMd167ContractComponent,
                nzContentParams: {
                    record
                }
            });

            drawerRef.afterClose.subscribe((data: any) => {
                if (data) {
                    let msg = data.Id ? `Sửa hợp đồng ${data.Code} thành công!` : `Thêm mới hợp đồng ${data.Code} thành công!`;
                    this.message.create('success', msg);
                    this.getData();
                }
            });
        }
    }

    async delete(data: any) {
        const resp = await this.md167ContractRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa hợp đồng ${data.Code} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    debt(record: any) {
        const drawerRef = this.drawerService.create<Md167ContractDebtTableComponent>({
            nzTitle: `Bảng công nợ ${record.Code}`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: Md167ContractDebtTableComponent,
            nzContentParams: {
                record
            }
        });
    }

    confirmDetele(data: any) {
        this.modalSrv.confirm({
            nzTitle: 'Xác nhận xóa hợp đồng ' + data.Code + ' ?',
            nzOnOk: () => {
                this.delete(data);
            },
            nzOkText: "Xác nhận",
            nzCancelText: 'Đóng'
        });
    }

    async getHouseData() {
        const resp = await this.md167ContractRepository.GetHouseData();

        if (resp.meta?.error_code == 200) {
            this.house_data = resp.data;
        }
    }

    import(importHistoryType: ImportHistoryTypeEnum) {
        let str = '';
        switch (importHistoryType) {
            case ImportHistoryTypeEnum.Md167Receipt:
                str = "phiếu thu";
                break;
                case ImportHistoryTypeEnum.Md167MainContract:
                str = "hợp đồng";
                break;
                case ImportHistoryTypeEnum.Md167ExtraContract:
                str = "phụ lục";
                break;
            default:
                break;
        }

        const drawerRef = this.drawerService.create<SharedImportExcelComponent>({
            nzTitle: `Import excel danh sách ${ str }`,
            nzWidth: '85vw',
            nzPlacement: 'left',
            nzContent: SharedImportExcelComponent,
            nzContentParams: {
                importHistoryType: importHistoryType
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            this.getData();
        });
    }
}
