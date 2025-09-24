import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { InitPricingComponent } from '../init-pricing/init-pricing.component';
import { PricingRepository } from 'src/app/infrastructure/repositories/pricing.repository';
import { TypeReportApply, TermApply } from 'src/app/shared/utils/consts';
import { TypeDecree, TypeAttributeCode, TypeReportApplyEnum, TypeEditHistoryEnum, AccessKey } from 'src/app/shared/utils/enums';
import { DeductionCoefficientRepository } from 'src/app/infrastructure/repositories/deduction-coefficient.repository';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { AddOrUpdatePricingComponent } from '../add-or-update/add-or-update-pricing.component';
import { VatRepository } from 'src/app/infrastructure/repositories/vat.repository';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { SharedConfirmUpdateMdComponent } from 'src/app/shared/components/confirm-update-md/confirm-update-md.component';
import { SharedConfirmUpdateListComponent } from 'src/app/shared/components/confirm-update-list/confirm-update-list.component';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-pricing',
  templateUrl: './pricing.component.html'
})
export class PricingComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  TypeReportApply = TypeReportApply;
  TypeReportApplyEnum = TypeReportApplyEnum;

  

  // decree_type1_data: any[] = [];
  typehouse_data: any[] = [];
  vat_data: any[] = [];
  customer_data: any[] = [];
  deduction_coefficient_data: any[] = [];
  role = this.commonService.CheckAccessKeyRole(AccessKey.PRICING);
  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    // { title: 'Nghị định áp dụng cho biên bản', index: 'decree.Code', width: 150 },
    // { title: 'Điều khoản áp dụng', index: 'TermApply', type: 'enum', enum: TermApply, width: 150 },
    { title: 'Ngày ra biên bản', index: 'DateCreate', type: 'date', className: 'text-center', width: 100, dateFormat: 'dd/MM/yyyy' },
    { title: 'Loại biên bản áp dụng', index: 'TypeReportApply', type: 'enum', enum: TypeReportApply, width: 220 },
    { title: 'Căn nhà số', index: 'block.Address', width: 150 },
    { title: 'Người mua', index: 'DecreeType2Name', width: 150 },
    { title: 'Mã căn hộ (Mã định danh)', index: 'apartment.Code', width: 150 },
    { title: 'Địa chỉ căn hộ', index: 'apartment.Address', width: 250 },
    { title: 'Tổng S xây dựng (m2)', render: 'cav-column', width: 100, className: 'text-center' },
    { title: 'Tổng S sử dụng (m2)', render: 'uav-column', width: 100, className: 'text-center' },
    { title: 'Diện tích đất ở', render: 'lcav-column', width: 100, className: 'text-center' },
    { title: 'Giá nhà', render: 'aprClmn', width: 150, fixed: 'right', className: 'text-right' },
    { title: 'Giá đất', render: 'lpClmn', width: 150, fixed: 'right', className: 'text-right' },
    { title: 'Tổng giá bán', render: 'tpClmn', width: 150, fixed: 'right', className: 'text-right' },
    {
      title: 'Chức năng',
      width: 170,
      className: 'text-center',
      fixed: 'right',
      buttons: [
        {
          icon: 'file-word',
          iif: i => i.TypeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE,
          click: record => this.export(record.Id),
          tooltip: "Xuất biên bản tính giá"
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
          click: record => this.update(record, undefined, true),
          tooltip: 'Xem'
        },
        {
          icon: 'edit',
          iif: i => !i.edit && this.role.Update,
          click: record => this.updateConfirm(record),
          tooltip: "Sửa"
        },
        {
          icon: 'delete',
          type: 'del',
          iif: i => this.role.Delete,
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
 
  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private pricingRepository: PricingRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private deductionCoefficientRepository: DeductionCoefficientRepository,
    private vatRepository: VatRepository,
    private typeBlockRepository: TypeBlockRepository,
    private customerRepository: CustomerRepository,
    private commonService: CommonService,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    // this.getDataDecreeType1();
    this.getTypeBlockData();
    this.getVatData();
    this.getCustomerData();
    this.getDeductionCoefficientData();
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
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Name.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TypeReportApply=${this.query.type}`
      console.log("TypeReportApply", TypeReportApply);
      console.log("TypeReportApplyEnum", TypeReportApplyEnum);
    }

    try {
      this.loading = true;
      const resp = await this.pricingRepository.getByPage(this.paging);

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

  openInitPricingModal(): void {
    this.modalSrv.create({
      nzTitle: `Thực hiện tính giá căn cứ theo`,
      nzContent: InitPricingComponent,
      nzWidth: 650,
      nzComponentParams: {
        typehouse_data: this.typehouse_data,
        // decree_type1_data: this.decree_type1_data
      },
      nzOnOk: (res: any) => {
        console.log(res);
        const drawerRef = this.drawerService.create<AddOrUpdatePricingComponent>({
          nzTitle: 'Thêm mới tính giá bán Nhà ở cũ',
          // record.khoa_chinh
          nzWidth: '85vw',
          nzContent: AddOrUpdatePricingComponent,
          nzPlacement: 'left',
          nzContentParams: {
            record: undefined,
            init_pricing: res.data,
            typehouse_data: res.typehouse_data,
            vat_data: this.vat_data,
            customer_data: this.customer_data,
            deduction_coefficient_data: this.deduction_coefficient_data
          }
        });

        drawerRef.afterClose.subscribe((data: any) => {
          if (data) {
            let msg = `Thêm mới biên bản tính giá thành công!`;
            this.message.success(msg);
            this.getData();
          }
        });
      }
    });
  }

  async delete(data: any) {
    const resp = await this.pricingRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa biên bản tính giá thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  // async getDataDecreeType1() {
  //   let paging: GetByPageModel = new GetByPageModel();
  //   paging.page_size = 0;
  //   paging.query = `TypeDecree=${TypeDecree.NGHIDINH}`;
  //   paging.select = 'Id,Code';

  //   const resp = await this.decreeRepository.getByPage(paging, TypeDecree.NGHIDINH);

  //   if (resp.meta?.error_code == 200) {
  //     this.decree_type1_data = resp.data;
  //   }
  // }

  async getTypeBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.typeBlockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    }
  }

  async getVatData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Value';

    const resp = await this.vatRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.vat_data = resp.data;
    }
  }

  async getCustomerData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,FullName';

    const resp = await this.customerRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.customer_data = resp.data;
      this.customer_data.map(item => {
        item.CustomerId = item.Id;
        return item;
      });
    }
  }

  update(data: any, editHistory?: any, isViewRecord?: boolean) {
    const drawerRef = this.drawerService.create<AddOrUpdatePricingComponent>({
      nzTitle: isViewRecord ? 'Xem thông tin biên bản tính giá bán NOC': 'Sửa thông tin biên bản tính giá bán NOC',
      // record.khoa_chinh
      nzWidth: '85vw',
      nzContent: AddOrUpdatePricingComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record: data,
        init_pricing: undefined,
        typehouse_data: this.typehouse_data,
        vat_data: this.vat_data,
        customer_data: this.customer_data,
        deduction_coefficient_data: this.deduction_coefficient_data,
        editHistory,
        isViewRecord
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = `Sửa biên bản tính giá thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async getDeductionCoefficientData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Value';

    const resp = await this.deductionCoefficientRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.deduction_coefficient_data = resp.data;
    }
  }

  export(id: number) {
    this.pricingRepository.exportReport(id);
  }

  updateConfirm(record: any) {
    this.modalSrv.create({
      nzTitle: `Xác nhận sửa biên bản tính giá nhà`,
      nzContent: SharedConfirmUpdateMdComponent,
      nzAutofocus: null,
      nzComponentParams: {
      },
      nzOnOk: async (res: any) => {
        let data = res.validateForm.value;
        this.update(record, data);
      }
    });
  }

  viewUpdateHistory(record: any) {
    this.modalSrv.create({
      nzTitle: `Lịch sử sửa biên bản tính giá nhà`,
      nzContent: SharedConfirmUpdateListComponent,
      nzAutofocus: null,
      nzComponentParams: {
        TypeEditHistoryEnum: TypeEditHistoryEnum.SELL_PRICING,
        targetId: record.Id
      },
      nzWidth: "1000px"
    });
  }
}
