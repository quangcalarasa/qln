import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { LogActionRepository } from 'src/app/infrastructure/repositories/log-action.repository';
import { TypeLogAction } from 'src/app/shared/utils/consts';

@Component({
    selector: 'app-log-action',
    templateUrl: './log-action.component.html'
})
export class LogActionComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;
    typeLogAction = TypeLogAction;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Người thực hiện', index: 'CreatedBy' },
        { title: 'Nội dung thực hiện', index: 'ActionName' },
        { title: 'Địa chỉ IP', index: 'IpAddress', width: 200 },
        { title: 'Bảng', index: 'TableName', width: 150 },
        { title: 'TargetId', index: 'TargetId', width: 100 },
        { title: 'Thời gian thực hiện', index: 'CreatedAt', type: 'date', width: 200, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm' },
        { title: 'Loại', index: 'Type', type: 'enum', enum: TypeLogAction, width: 150 }
    ];

    constructor(
        private modalSrv: NzModalService,
        private logActionRepository: LogActionRepository
    ) { }

    ngOnInit(): void {
        this.getData();
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
                this.paging.query += ` and (ActionName.Contains("${this.query.txtSearch}")` + ` or CreatedBy.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and Type=${this.query.type}`
        }

        if (this.query.dateStart && this.query.dateStart != "") {
            let splDateStart = this.query.dateStart.split("-");
            this.paging.query += `and CreatedAt >= DateTime(${splDateStart[0]},${splDateStart[1]},${splDateStart[2]},0,0,0)`;
        }

        if (this.query.dateEnd && this.query.dateEnd != "") {
            let splDateEnd = this.query.dateEnd.split("-");
            this.paging.query += `and CreatedAt <= DateTime(${splDateEnd[0]},${splDateEnd[1]},${splDateEnd[2]},23,59,59)`;
        }

        try {
            this.loading = true;
            const resp = await this.logActionRepository.getByPage(this.paging);

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
}
