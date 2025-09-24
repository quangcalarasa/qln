import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { LogActionRepository } from 'src/app/infrastructure/repositories/log-action.repository';
import { TypeLogAction } from 'src/app/shared/utils/consts';
import { Router } from '@angular/router';

@Component({
    selector: 'app-extra-info-debt-info',
    templateUrl: './debt-info.component.html'
})
export class DebtInfoComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;

    title: string = "";

    typeComponent?: number = undefined;

    columns: STColumn[];

    columns_type1: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Mã định danh', index: 'Code' },
        { title: 'Căn nhà', index: 'House' },
        { title: 'Căn hộ', index: 'Apartment' },
        { title: 'Địa chỉ', index: 'Address' },
        { title: 'Diện tích', index: 'Area', className: 'text-center' },
        { title: 'Chủ căn hộ', index: 'Customer' },
        { title: 'Điện thoại', index: 'Phone' },
        { title: 'Trạng thái', index: 'Status' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type2: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Loại công nợ', index: 'Type' },
        { title: 'Biểu mẫu thông báo', index: 'Template' },
        { title: 'Tiêu đề', index: 'Title' },
        { title: 'Nội dung', index: 'Body' },
        { title: 'Đối tượng nhận thông báo', index: 'Receiver' },
        { title: 'Ngày giờ gửi', index: 'Time' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type3: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Ngày áp dụng', index: 'Date' },
        { title: 'Số ngày quá hạn', index: 'Count', className: "text-center" },
        { title: 'Người cập nhật', index: 'CreatedBy' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type4: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Ngày nhắc nợ', index: 'Date' },
        { title: 'Căn nhà', index: 'House' },
        { title: 'Căn hộ', index: 'Apartment' },
        { title: 'Địa chỉ', index: 'Address' },
        { title: 'Chủ hộ', index: 'Customer' },
        { title: 'Số điện thoại', index: 'Phone' },
        { title: 'Nội dung', index: 'Body' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type5: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Tiêu đề thông báo', index: 'Title' },
        { title: 'Nội dung thông báo', index: 'Body' },
        { title: 'Loại thông báo', index: 'Type' },
        { title: 'Ngày gửi thông báo', index: 'Date' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type6: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Căn nhà', index: 'House' },
        { title: 'Căn hộ', index: 'Apartment' },
        { title: 'Địa chỉ', index: 'Address' },
        { title: 'Chủ hộ', index: 'Customer' },
        { title: 'Số điện thoại', index: 'Phone' },
        { title: 'Kỳ đóng', index: 'Period', className: "text-center" },
        { title: 'Số tiền', index: 'Amount', className: "text-right" },
        { title: 'Ngày cần đóng', index: 'Date', className: "text-center" },
        { title: 'Số ngày quá hạn', index: 'Count', className: "text-center" },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    columns_type7: STColumn[] = [
        { title: 'Stt', render: 'no-column', width: 40 },
        { title: 'Ngày áp dụng', index: 'Date', className: "text-center" },
        { title: 'Số ngày quá hạn', index: 'Period', className: "text-center" },
        { title: 'Diễn giải', index: 'House' },
        { title: 'Người cập nhật', index: 'UpdatedBy' },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit
                },
                {
                    icon: 'delete',
                    type: 'del',
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá?',
                        okType: 'danger',
                        icon: 'star'
                    },
                }
            ]
        }
    ];

    constructor(
        private modalSrv: NzModalService,
        private router: Router
    ) { }

    ngOnInit(): void {
        let type: number = 0;
        let url = this.router.url;
        switch (url) {
            case "/extra-info/debt/track-house":
                this.title = "Lịch sử, tình trạng sử dụng của căn nhà"
                this.columns = this.columns_type1;
                type = 1;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/notification":
                this.title = "Thông báo công nợ"
                this.columns = this.columns_type2;
                type = 2;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/setting-notification":
                this.title = "Cấu hình gửi thông báo công nợ"
                this.columns = this.columns_type3;
                type = 3;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/reminder":
                this.title = "Quản lý nhắc nợ"
                this.columns = this.columns_type4;
                type = 4;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/view-notification":
                this.title = "Xem thông báo đã gửi"
                this.columns = this.columns_type5;
                type = 5;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/overdue":
                this.title = "Quản lý nợ quá hạn"
                this.columns = this.columns_type6;
                type = 6;
                this.data  = this.getData(type);
                break;
            case "/extra-info/debt/setting-reminder":
                this.title = "Cấu hình nhắc nợ"
                this.columns = this.columns_type7;
                type = 7;
                this.data  = this.getData(type);
                break;
            default:
                break;
        }

        this.getData(type);

    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                break;
            case 'dblClick':
                break;
            case 'checkbox':
                break;
            case 'sort':
                this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
                break;
            default:
                break;
        }
    }

    reset(): void {
        this.query = new QueryModel();
        this.paging.page = 1;
    }

    searchData() {
        this.paging.page = 1;
    }

    getData(type: number): any[] {
        if (type == 1) {
            return [
                { Code: "CH001", House: "9/4", Apartment: "102A", Address: "102A", Area: "86", Customer: "Nguyễn Hữu Dũng", Phone: "", Status: "Đang cho thuê" },
                { Code: "CH002", House: "31/12", Apartment: "151/1", Address: "151/1", Area: "90", Customer: "Trần Văn Thành", Phone: "", Status: "Đang cho thuê" },
                { Code: "CH00209", House: "201 lô I Chung cư Ngô Gia Tự", Apartment: "2308", Address: "2308", Area: "110", Customer: "Nguyễn Thanh Phong", Phone: "", Status: "Đã bán" },
                { Code: "CH0034", House: "B112/7", Apartment: "102", Address: "102", Area: "90", Customer: "Vi Mạnh Tuyên", Phone: "", Status: "Đã bán" },
                { Code: "CH059", House: "02/01", Apartment: "234", Address: "234", Area: "120", Customer: "Đình Huệ", Phone: "", Status: "Đã bán" },
                { Code: "CH001222", House: "Số 9 TCV", Apartment: "252", Address: "252", Area: "150", Customer: "Hoàng Quốc Thái", Phone: "", Status: "Đang cho thuê" },
                { Code: "CH00123", House: "1/5", Apartment: "082", Address: "082", Area: "120", Customer: "Nguyễn Thành Lân", Phone: "", Status: "Đang cho thuê" },
                { Code: "CH001090", House: "7/4", Apartment: "4b", Address: "4b", Area: "115", Customer: "Nguyễn Thị Lan Anh", Phone: "", Status: "Đã bán" },
                { Code: "CH0012233", House: "9/2", Apartment: "1701", Address: "1701", Area: "100", Customer: "Trần Hữu Phước", Phone: "", Status: "Đang cho thuê" },
            ];
        }
        else if(type == 2) {
            return [
                { Type: "Nợ thuê nhà", Template: "BM001", Title: "Thông báo nợ", Body: "Gửi thông báo nợ tới căn hộ, chi tiết trong file đính kèm trong email", Receiver: "Căn nhà", Time: "20/10/2019 10:00" },
                { Type: "Nợ bán trả góp 1 lần", Template: "BM002", Title: "Thông báo nợ", Body: "Gửi thông báo nợ tới căn hộ, chi tiết trong file đính kèm trong email", Receiver: "Căn nhà", Time: "10/11/20120 10:00" },
                { Type: "Nợ thuê nhà", Template: "BM005", Title: "Thông báo nợ", Body: "Gửi thông báo nợ tới căn hộ, chi tiết trong file đính kèm trong email", Receiver: "Căn nhà", Time: "15/08/2021 10:00" },
                { Type: "Nợ thuê nhà", Template: "BM002", Title: "Thông báo nợ", Body: "Gửi thông báo nợ tới căn hộ, chi tiết trong file đính kèm trong email", Receiver: "Căn nhà", Time: "20/10/2023 10:00" },
            ];
        }
        else if(type == 3) {
            return [
                { Date: "08/08/2022", Count: "3", CreatedBy: "admin" },
                { Date: "10/01/2021", Count: "5", CreatedBy: "cb.tdc" },
                { Date: "12/12/2019", Count: "5", CreatedBy: "cb.noc" },
            ];
        }
        else if(type == 4) {
            return [
                { Date: "13/05/2022", House: "1/5", Apartment: "082", Address: "082", Customer: "Nguyễn Thành Lân", Phone: "", Body: "Nhắc nợ của hợp đồng mua nhà trả góp" }
            ];
        }
        else if(type == 5) {
            return [
                { Title: "Gửi thông báo nhắc nợ", Body: "Chi tiết trong file đính kèm trong email", Type: "Thông báo nhắc nợ", Date: "10/02/2022" }
            ];
        }
        else if(type == 6) {
            return [
                { House: "CH0012233", Apartment: "9/2", Address: "1701", Customer: "Trần Hữu Phước", Phone: "", Period: "3 tháng", Amount: "3.450.000", Date: "20/10/2022" , Count: "5"}
            ];
        }
        else if(type == 7) {
            return [
                { Date: "13/05/2022", Period: "2 tháng 1 lần", House: "Lorem inout type submit", UpdatedBy: "admin_clone" }
            ];
        }
        else return [];
    }

    onBack() {
        window.history.back();
    }
}
