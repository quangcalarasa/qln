import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TypeReportApply } from 'src/app/shared/utils/consts';
import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { AddOrUpdateRentFileComponent } from './add-or-update-rent-file/add-or-update-rent-file.component';
import { RentTableComponent } from './rent-table/rent-table.component';
import { DebtComponent } from './debt/debt.component';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { BctForUserComponent } from './bct-for-user/bct-for-user.component';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { TypeAttributeCode, ImportHistoryTypeEnum, TypeEditHistoryEnum, AccessKey } from 'src/app/shared/utils/enums';
import { SharedImportExcelComponent } from 'src/app/shared/components/import-excel/import-excel.component';
import { ExportPromissoryComponent } from 'src/app/routes/cate-rent-noc/rent-file/export-promissory/export-promissory.component';
import { SharedConfirmUpdateMdComponent } from 'src/app/shared/components/confirm-update-md/confirm-update-md.component';
import { SharedConfirmUpdateListComponent } from 'src/app/shared/components/confirm-update-list/confirm-update-list.component';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-rent-file',
  templateUrl: './rent-file.component.html',
  styles: []
})
export class RentFileComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  typeReportApply = TypeReportApply;

  typehouse_data: any[] = [];
  contractStatus: any[] = [];
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENT_FILE);
  roleDebt = this.commonService.CheckAccessKeyRole(AccessKey.DEBT_RENT_CONTRACT_NOC);
  roleImportRentContract = this.commonService.CheckAccessKeyRole(AccessKey.IMPORT_RENT_CONTRACT_NOC);
  roleExportReceipt = this.commonService.CheckAccessKeyRole(AccessKey.EXPORT_RECEIPT_CONTRACT_NOC);
  roleImportReceipt = this.commonService.CheckAccessKeyRole(AccessKey.IMPORT_RECEIPT_CONTRACT_NOC);

  
  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Số hồ sơ', index: 'CodeHS' },
    { renderTitle: 'ucodeHeader', render: 'code', className: 'text-center' },
    { title: 'Số hợp đồng', index: 'Code' },
    { title: 'Trạng thái hợp đồng', render: 'fileStatus-column' },
    { title: 'Loại nhà', index: 'TypeBlockId', render: 'typeblock-column' },
    { title: 'Ngày ký hợp đồng', index: 'DateHD', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Ngày cập nhật', index: 'UpdatedAt', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { title: 'Người cập nhật', index: 'UpdatedBy' },
    { renderTitle: 'useAreaValueHeader', render: 'uavClmn', className: 'text-center' },
    // { title: 'Chức năng', render: 'function-column', className: 'text-center', width: 120 },
    {
      title: 'Chức năng',
      width: 130,
      className: 'text-center',
      buttons: [
        {
          icon: 'table',
          iif: i => !i.edit,
          click: record => this.getWorkSheet(record),
          tooltip: 'Bảng chiết tính'
        },
        {
          icon: 'copy',
          iif: i => !i.edit,
          click: record => this.getWorkSheetClone(record),
          tooltip: 'Bảng chiết tính User'
        },
        {
          icon: 'credit-card',
          iif: i => !i.edit && this.roleDebt.ViewOrActionSpecial,
          click: record => this.debt(record),
          tooltip: 'Công nợ'
        },
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
          icon: 'delete',
          iif: i=> this.role.Delete,
          type: 'del',
          tooltip: 'Xóa',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá hợp đồng này?',
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
    private rentFileRepository: RentFileRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private TypeBlockRepository: TypeBlockRepository,
    private typeAttributeRepository: TypeAttributeRepository,
    private commonService: CommonService,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getTypeHouse();
    this.getItemByTypeAttributeCode();
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
    this.paging.page_size = 10;
    this.getData();
  }

  searchData() {
    this.paging.page = 1;
    this.getData();
  }

  async getData() {
    this.paging.query = `Type=1`;
    this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and (Code.Contains("${this.query.txtSearch}") or CodeCN.Contains("${this.query.txtSearch}") or CodeCH.Contains("${this.query.txtSearch}"))`;

    }
    if (this.query.type != undefined) {
      this.paging.query += ` and FileStatus=${this.query.type}`;
    }
    try {
      this.loading = true;
      const resp = await this.rentFileRepository.getByPage(this.paging);

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

  addOrUpdate(record?: any, block_rent?: any, code?: string, editHistory?: any, isViewRecord?: boolean): void {
    const drawerRef = this.drawerService.create<AddOrUpdateRentFileComponent>({
      nzTitle: record ? (isViewRecord ? `Xem hồ sơ giá thuê: ${record.Code}` : `Sửa hồ sơ giá thuê: ${record.Code}`) : 'Thêm mới hồ sơ giá thuê',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateRentFileComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        block_rent,
        typehouse_data: this.typehouse_data,
        code,
        contractStatus: this.contractStatus,
        editHistory,
        isViewRecord
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hồ sơ giá thuê ${data.Code} thành công!` : `Thêm mới hồ sơ giá thuê ${data.Code} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  getWorkSheet(record?: any): void {
    const drawerRef = this.drawerService.create<RentTableComponent>({
      nzTitle: 'Bảng chiết tính',
      nzWidth: '100vw',
      nzContent: RentTableComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  getWorkSheetClone(record?: any): void {
    const drawerRef = this.drawerService.create<BctForUserComponent>({
      nzTitle: 'Bảng chiết tính',
      nzWidth: '100vw',
      nzContent: BctForUserComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  debt(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<DebtComponent>({
      nzTitle: 'Công nợ',
      nzWidth: '85vw',
      nzContent: DebtComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  async delete(data: any) {
    const resp = await this.rentFileRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hồ sơ giá thuê ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
  async getTypeHouse() {
    let paging: GetByPageModel = new GetByPageModel();

    const resp = await this.TypeBlockRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    }
  }

  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find(x => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  async getItemByTypeAttributeCode() {
    const resp = await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.TRANGTHAI_HOPDONG_THUE_NOC);

    if (resp.meta?.error_code == 200) {
      this.contractStatus = resp.data;
    }
  }

  findContractStatus(id: number) {
    let ti = this.contractStatus.find(x => x.Id == id);
    return ti ? ti.Name : undefined;
  }

  import() {
    this.drawerService.create<SharedImportExcelComponent>({
      nzTitle: `Import excel phiếu thu cho hợp đồng thuê (NOC)`,
      nzWidth: '85vw',
      nzPlacement: 'left',
      nzContent: SharedImportExcelComponent,
      nzContentParams: {
        importHistoryType: ImportHistoryTypeEnum.Noc_Contract_Rent_Receipt
      }
    });
  }

  importContract() {
    const drawerRef = this.drawerService.create<SharedImportExcelComponent>({
      nzTitle: `Import excel hợp đồng thuê (NOC)`,
      nzWidth: '85vw',
      nzPlacement: 'left',
      nzContent: SharedImportExcelComponent,
      nzContentParams: {
        importHistoryType: ImportHistoryTypeEnum.Noc_Contract_Rent
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      this.getData();
    });
  }

  exportPromissory() {
    this.drawerService.create<ExportPromissoryComponent>({
      nzTitle: `Xuất phiếu thu hợp đồng thuê`,
      nzWidth: '100vw',
      nzPlacement: 'left',
      nzContent: ExportPromissoryComponent,
      nzContentParams: {
        code: ""
      }
    });
  }

  updateConfirm(record: any) {
    this.modalSrv.create({
      nzTitle: `Xác nhận sửa hồ sơ thuê`,
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
      nzTitle: `Lịch sử sửa hồ sơ thuê`,
      nzContent: SharedConfirmUpdateListComponent,
      nzAutofocus: null,
      nzComponentParams: {
        TypeEditHistoryEnum: TypeEditHistoryEnum.RENT_CONTRACT,
        targetId: record.Id
      },
      nzWidth: "1000px"
    });
  }
}
