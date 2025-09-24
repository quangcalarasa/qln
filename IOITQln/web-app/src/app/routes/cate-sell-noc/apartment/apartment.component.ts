import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AddOrUpdateApartmentComponent } from './add-or-update/add-or-update-apartment.component';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import { TypeReportApply, Decree } from 'src/app/shared/utils/consts';
import { SearchRentApartmentComponent } from './search-rent-apartment/search-rent-apartment.component';
import { CodeStatusEnum, TypeApartmentEntityEnum, TypeReportApplyEnum, TypeEditHistoryEnum, AccessKey } from 'src/app/shared/utils/enums';
import { SharedConfirmUpdateMdComponent } from 'src/app/shared/components/confirm-update-md/confirm-update-md.component';
import { SharedConfirmUpdateListComponent } from 'src/app/shared/components/confirm-update-list/confirm-update-list.component';
import { CommonService } from 'src/app/core/services/common.service';
import { NocApartmentFileComponent } from './noc-apartment-file/noc-apartment-file.component';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';

@Component({
    selector: 'app-cate-apartment',
    templateUrl: './apartment.component.html'
})
export class ApartmentComponent implements OnInit {
    @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

    validateForm!: FormGroup;
    paging: GetByPageModel = new GetByPageModel();
    query: QueryModel = new QueryModel();

    data: any[] = [];
    loading = false;
    role = this.commonService.CheckAccessKeyRole(AccessKey.APARTMENT_MANAGEMENT);
    TypeReportApply = TypeReportApply;
    Decree = Decree;
    TypeReportApplyEnum = TypeReportApplyEnum;

    filter = false;

    districts: any;
    wards: any;
    lanes: any;

    columns: STColumn[] = [
        // { title: '', index: 'Id', type: 'checkbox' },
        { title: 'Stt', type: 'no', width: 40 },
        { title: 'Loại biên bản áp dụng', index: 'TypeReportApply', type: 'enum', enum: TypeReportApply },
        { title: 'Căn nhà', index: 'BlockName' },
        { title: 'Mã định danh', index: 'Code' },
        { title: 'Địa chỉ căn hộ', index: 'Address' },
        { title: 'Tổng diện tích xây dựng', render: 'cavClmn', className: "text-center" },
        { title: 'Tổng diện tích sử dụng', render: 'uavClmn', className: "text-center" },
        { title: 'Diện tích đất ở', render: 'lcavClmn', className: "text-center" },
        {
            title: 'Chức năng',
            width: 130,
            className: 'text-center',
            buttons: [
                {
                    icon: 'history',
                    iif: i => !i.edit,
                    click: record => this.viewUpdateHistory(record),
                    tooltip: 'Lịch sử cập nhật'
                },
                {
                    icon: 'eye',
                    iif: i => !i.edit,
                    click: record => this.addOrUpdate(record, undefined, undefined, undefined, true),
                    tooltip: 'Xem'
                },
                {
                    icon: 'edit',
                    click: record => this.updateConfirm(record),
                    tooltip: 'Sửa',
                    iif: i => !i.edit && this.role.Update,
                },
                {
                    icon: 'import',
                    click: record => this.ApartmentFile(record),
                    tooltip: 'Đính kèm file',
                },
                {
                    icon: 'delete',
                    type: 'del',
                    tooltip: 'Xóa',
                    iif: i => this.role.Delete,
                    pop: {
                        title: 'Bạn có chắc chắn muốn xoá căn hộ này?',
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
        private apartmentRepository: ApartmentRepository,
        private drawerService: NzDrawerService,
        private message: NzMessageService,
        private commonService: CommonService,
        private wardRepository: WardRepository,
        private laneRepository: LaneRepository,
        private districtRepository: DistrictRepository
    ) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({});
        this.getData();
        this.getDataDistrict();
        this.getDataWard();
        this.getDataLane();
    }

    tableRefChange(e: STChange): void {
        switch (e.type) {
            case 'pi':
                this.paging.page = e.pi;
                this.getData();
                break;
            case 'dblClick':
                // this.addOrUpdate(e.dblClick?.item);
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
        this.paging.query = `TypeApartmentEntity=${TypeApartmentEntityEnum.APARTMENT_NORMAL}`;
        this.paging.order_by = 'CreatedAt Desc';

        if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
            if (this.query.txtSearch.trim() != '')
                // this.paging.query += ` and (Address.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
                this.paging.query += ` and (Address.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}") or BlockAddress.Contains("${this.query.txtSearch}"))`;
        }

        if (this.query.type != undefined) {
            this.paging.query += ` and TypeReportApply=${this.query.type}`
        }

        if (this.query.type2 != undefined) {
            this.paging.query += ` and District=${this.query.type2}`
        }

        if (this.query.type3 != undefined) {
            this.paging.query += ` and Ward=${this.query.type3}`
        }

        if (this.query.type4 != undefined) {
            this.paging.query += ` and Lane=${this.query.type4}`
        }

        try {
            this.loading = true;
            const resp = await this.apartmentRepository.getByPage(this.paging, this.query.type1);

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

    addOrUpdate(record?: any, apartment_rent?: any, code?: string, editHistory?: any, isViewRecord?: boolean): void {
        const drawerRef = this.drawerService.create<AddOrUpdateApartmentComponent>({
            nzTitle: record ? (isViewRecord ? `Xem căn hộ: ${record.Code}` : `Sửa căn hộ: ${record.Code}`) : 'Thêm mới căn hộ',
            // record.khoa_chinh
            nzWidth: '85vw',
            nzContent: AddOrUpdateApartmentComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record,
                apartment_rent,
                code,
                editHistory,
                isViewRecord
            }
        });

        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Sửa căn hộ ${data.Code} thành công!` : `Thêm mới căn hộ ${data.Code} thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async delete(data: any) {
        const resp = await this.apartmentRepository.delete(data);
        if (resp.meta?.error_code == 200) {
            this.message.create('success', `Xóa căn hộ ${data.Code} thành công!`);
            this.getData();
        } else {
            this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
        }
    }

    onBack() {
        window.history.back();
    }

    searchRentBlock() {
        this.modalSrv.create({
            nzTitle: `Tra cứu thông tin Mã định danh căn hộ`,
            nzContent: SearchRentApartmentComponent,
            nzComponentParams: {
            },
            nzOnOk: async (res: any) => {
                let codeStatus = res.codeStatus;
                if (codeStatus == CodeStatusEnum.CHUA_TON_TAI) {
                    let code = res.validateForm.value.Code;
                    this.addOrUpdate(undefined, undefined, code);
                }
                else if (codeStatus == CodeStatusEnum.DA_TON_TAI) {
                    let apartmentId = res.data?.Id;

                    const resp = await this.apartmentRepository.getById(apartmentId);
                    if (resp.meta?.error_code == 200) {
                        let apartment_rent = resp.data;
                        this.addOrUpdate(undefined, apartment_rent);
                    }
                }
                else if (codeStatus == CodeStatusEnum.DA_CAP_NHAT) {
                    let apartmentId = res.data?.Id;

                    const resp = await this.apartmentRepository.getById(apartmentId);
                    if (resp.meta?.error_code == 200) {
                        let apartment = resp.data;
                        this.addOrUpdate(apartment);
                    }
                }
            }
        });
    }

    updateConfirm(record: any) {
        this.modalSrv.create({
            nzTitle: `Xác nhận sửa căn hộ ${record.Address}`,
            nzContent: SharedConfirmUpdateMdComponent,
            nzAutofocus: null,
            nzComponentParams: {
            },
            nzOnOk: async (res: any) => {
                let data = res.validateForm.value;
                this.addOrUpdate(record, undefined, undefined, data);
            }
        });
    }

    viewUpdateHistory(record: any) {
        this.modalSrv.create({
            nzTitle: `Lịch sử sửa căn hộ ${record.Address}`,
            nzContent: SharedConfirmUpdateListComponent,
            nzAutofocus: null,
            nzComponentParams: {
                TypeEditHistoryEnum: TypeEditHistoryEnum.APARTMENT,
                targetId: record.Id
            },
            nzWidth: "1000px"
        });
    }

    ApartmentFile(record?: any): void {
        let add;
        record ? (add = true) : (add = false);
        localStorage.setItem('add', add.toString());
        const drawerRef = this.drawerService.create<NocApartmentFileComponent>({
            nzTitle: 'Danh sách file đính kèm',
            // record.khoa_chinh
            nzWidth: '75vw',
            nzContent: NocApartmentFileComponent,
            nzPlacement: 'left',
            nzContentParams: {
                record
            }
        });
        drawerRef.afterClose.subscribe((data: any) => {
            if (data) {
                let msg = data.Id ? `Thêm file thành công!` : ` Thêm file thành công!`;
                this.message.success(msg);
                this.getData();
            }
        });
    }

    async getDataDistrict() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = 'Id,Name';
        paging.query = `ProvinceId=2`;

        const respDistrict = await this.districtRepository.getByPage(paging);
        if (respDistrict.meta?.error_code == 200) {
            this.districts = respDistrict.data;
        }
    }

    async getDataWard() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = 'Id,Name,DistrictId';
        paging.query = `ProvinceId=2`;

        const respWard = await this.wardRepository.getByPage(paging);
        if (respWard.meta?.error_code == 200) {
            this.wards = respWard.data;
        }
    }

    async getDataLane() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = 'Id,Name,Ward,District';
        paging.query = `Province=2`;

        const respLane = await this.laneRepository.getByPage(paging);
        if (respLane.meta?.error_code == 200) {
            this.lanes = respLane.data;
        }
    }

    changeDistrict() {
        this.query.type4 = undefined;
        this.query.type3 = undefined;
    }

    changeWard() {
        this.query.type4 = undefined;
    }
}
