import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { AddOrUpdateNo2LandPriceComponent } from './add-or-update/add-or-update-no2-land-price.component';
import { No2LandPriceRepository } from 'src/app/infrastructure/repositories/no2-land-price.repository';
import { AccessKey } from 'src/app/shared/utils/enums';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
    selector: 'app-cate-no2-land-price',
    templateUrl: './no2-land-price.component.html'
})
export class No2LandPriceComponent implements OnInit {
    // @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;
    role = this.commonService.CheckAccessKeyRole(AccessKey.NO2_LAND_PRICE_MANAGEMENT);
    columns: STColumn[] = [
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Ghi chú', index: 'Note', width: 250 },
        { title: 'Giá trị bắt đầu', render: 'sValueClmn', className: 'text-right' },
        { title: 'Giá trị kết thúc', render: 'eValueClmn', className: 'text-right' },
        {
            title: 'Chiều rộng hẻm lớn hơn 5 mét', children: [
                { title: 'Đất nằm mặt tiền hẻm chính', render: 'mpg5mValueClmn', className: 'text-right' },
                { title: 'Đất nằm ở hẻm phụ', render: 'epg5mValueClmn', className: 'text-right' }
            ]
        },
        {
            title: 'Chiều rộng hẻm từ 5 mét đến 3 mét', children: [
                { title: 'Đất nằm mặt tiền hẻm chính', render: 'mpl5mValueClmn', className: 'text-right' },
                { title: 'Đất nằm ở hẻm phụ', render: 'epl5mValueClmn', className: 'text-right' }
            ]
        },
        {
            title: 'Chiều rộng hẻm từ 3 mét đến 2 mét', children: [
                { title: 'Đất nằm mặt tiền hẻm chính', render: 'mpl3mValueClmn', className: 'text-right' },
                { title: 'Đất nằm ở hẻm phụ', render: 'epl3mValueClmn', className: 'text-right' }
            ]
        },
        {
            title: 'Chiều rộng hẻm nhỏ hơn 2 mét', children: [
                { title: 'Đất nằm mặt tiền hẻm chính', render: 'mpl2mValueClmn', className: 'text-right' },
                { title: 'Đất nằm ở hẻm phụ', render: 'epl2mValueClmn', className: 'text-right' }
            ]
        },
        {
            title: 'Chức năng',
            width: 100,
            className: 'text-center',
            buttons: [
                {
                    icon: 'edit',
                    iif: i => !i.edit && this.role.Update,
                    click: record => this.addOrUpdate(record)
                },
                {
                    icon: 'delete',
                    type: 'del',
                    iif: i => this.role.Delete,
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá thông tin giá đất này?',
                        okType: 'danger',
                        icon: 'star'
                    },
                    click: record => this.delete(record)
                }
            ]
        }
    ];

    constructor(
        private modalSrv: NzModalService,
        private no2LandPriceRepository: No2LandPriceRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private commonService: CommonService,
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
        this.paging.order_by = 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                this.paging.query += ` and Note.Contains("${this.query.txtSearch}")`;
        }

        try {
            this.loading = true;
            const resp = await this.no2LandPriceRepository.getByPage(this.paging);

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
        const drawerRef = this.drawerService.create<AddOrUpdateNo2LandPriceComponent>({
            nzTitle: record ? `Sửa thông tin giá đất` : 'Thêm mới thông tin giá đất',
            // record.khoa_chinh
            nzWidth: '75vw',
            nzContent: AddOrUpdateNo2LandPriceComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa thông tin giá đất thành công!` : `Thêm mới thông tin giá đất thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.no2LandPriceRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa thông tin giá đất thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }
}
