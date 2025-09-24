import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateHolidayComponent } from './add-or-update/add-or-update-holiday.component';
import { HolidayRepository } from 'src/app/infrastructure/repositories/holiday.repository';
import { CalendarComponent } from './calendar/calendar.component';
import { AgGridComponent } from './ag-grid/ag-grid.component';

@Component({
    selector: 'app-holiday',
    templateUrl: './holiday.component.html'
})
export class HolidayComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    dataAll: any[] = [];
    loading = false;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Tên kỳ nghỉ lễ', index: 'Name' },
        { title: 'Từ ngày', index: 'StartDate', type: 'date', width: 90, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        { title: 'Đến ngày', index: 'EndDate', type: 'date', width: 90, sort: true, className: 'text-center', dateFormat: 'dd/MM/yyyy' },
        { title: 'Ghi chú', index: 'Note' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit,
                    click: record => this.addOrUpdate(record)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá kỳ nghỉ lễ này?',
                        okType: 'danger',
                        icon: 'star'
                    },
                    click: record => this.delete(record)
                }
            ]
        }
    ];

    constructor(
        private fb: FormBuilder,
        private modalSrv: NzModalService,
        private holidayRepository: HolidayRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService
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
                this.addOrUpdate(e.dblClick?.item);
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
                this.paging.query += ` and Name.Contains("${this.query.txtSearch}")`;
        }

        try {
            this.loading = true;
            const resp = await this.holidayRepository.getByPage(this.paging);

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

    addOrUpdate(record?: any): void {
        const drawerRef = this.drawerService.create<AddOrUpdateHolidayComponent>({
            nzTitle: record ? `Sửa kỳ nghỉ lễ: ${record.Name}` : 'Thêm mới kỳ nghỉ lễ',
            nzWidth: '55vw',
            nzContent: AddOrUpdateHolidayComponent,
            nzContentParams: {
                record
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa kỳ nghỉ lễ ${data.Name} thành công!` : `Thêm mới kỳ nghỉ lễ ${data.Name} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.holidayRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa kỳ nghỉ lễ ${data.Name} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    viewCalendar() {
        const drawerRef = this.drawerService.create<CalendarComponent>({
            nzTitle: 'Xem chi tiết các ngày nghỉ lễ trên lịch',
            nzWidth: '100vw',
            nzContent: CalendarComponent,
            nzContentParams: {
            }
        });
    }

    viewAgGrid() {
        const drawerRef = this.drawerService.create<AgGridComponent>({
            nzTitle: 'Mẫu báo cáo động',
            nzWidth: '100vw',
            nzContent: AgGridComponent,
            nzContentParams: {
            }
        });
    }
}
