import { ArrearsComponent } from './../arrears/arrears.component';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STComponent } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { TypeReportApply, LevelBlock, FileStatus } from 'src/app/shared/utils/consts';
import { MemberRentFileComponent } from '../member-rent-file/member-rent-file.component';
import { ApartmentRepository } from 'src/app/infrastructure/repositories/apartment.repository';
import { AddendumComponent } from '../addendum/addendum.component';
import { NzMessageService } from 'ng-zorro-antd/message';
import QueryModel from 'src/app/core/models/query-model';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { TypeHouse } from '../../../../shared/utils/consts';
import { ProcessProfileCeRepository } from '../../../../infrastructure/repositories/process-profile-ce.repository';
import { RentFileBCTRepository } from '../../../../infrastructure/repositories/rent-File-BCT.repository';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';
@Component({
  selector: 'app-add-or-update-rent-file',
  templateUrl: './add-or-update-rent-file.component.html',
  styles: []
})
export class AddOrUpdateRentFileComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;
  @ViewChild('MemberRentFileComponent') private MemberRentFileComponent!: MemberRentFileComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() block_rent: NzSafeAny;
  @Input() code: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;
  @Input() contractStatus: NzSafeAny;
  @Input() editHistory: NzSafeAny;
  @Input() isViewRecord?: boolean;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  customer_data: any[] = [];
  customer_data_filter: any[] = [];
  block_data: any[] = [];
  block_Filter_data: any[] = [];
  data_filter_block: any[] = [];

  apartment_data: any[] = [];
  data_filter_apartment: any[] = [];
  process_profile_ce_data: any[] = [];
  DataAddendum: any[] = [];
  role = this.commonService.CheckAccessKeyRole(AccessKey.RENT_FILE);
  level_data = LevelBlock;
  TypeReportApply = TypeReportApply;
  TypeHouse = TypeHouse;
  nzFormat = 'dd/ MM/ yyyy';
  change = false;
  change1 = false;
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private rentFileRepository: RentFileRepository,
    private modalSrv: NzModalService,
    private customerRepository: CustomerRepository,
    private blockRepository: BlockRepository,
    private apartmentRepository: ApartmentRepository,
    private message: NzMessageService,
    private drawerService: NzDrawerService,
    private processProfileCeRepository: ProcessProfileCeRepository,
    private rentFileBCTRepository: RentFileBCTRepository,
    private commonService: CommonService,
  ) {
    this.getProcessProfileCeData();
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      CustomerId: [this.record ? this.record.CustomerId : undefined, [Validators.required]],
      RentBlockId: [this.record ? this.record.RentBlockId : undefined, [Validators.required]],
      RentApartmentId: [this.record ? this.record.RentApartmentId : undefined],
      FileStatus: [this.record ? this.record.FileStatus : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      CodeHS: [this.record ? this.record.CodeHS : undefined, [Validators.required]],
      DateHD: [this.record ? convertDate(this.record.DateHD) : undefined],

      Dob: [this.record ? convertDate(this.record.Dob) : undefined],
      CodeKH: [this.record ? this.record.CodeKH : undefined],
      AddressKH: [this.record ? this.record.AddressKH : undefined],
      Phone: [this.record ? this.record.Phone : undefined],

      TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined],
      TypeBlockId: [this.record ? this.record.TypeBlockId : undefined],
      CampusArea: [this.record ? this.record.CampusArea : undefined],
      fullAddressCN: [this.record ? this.record.fullAddressCN : undefined],
      ConstructionAreaValue: [this.record ? this.record.ConstructionAreaValue : undefined],
      UseAreaValueCN: [this.record ? this.record.UseAreaValueCN : undefined],
      CodeCN: [this.record ? this.record.CodeCN : undefined],

      CodeCH: [this.record ? this.record.CodeCH : undefined],
      UseAreaValueCH: [this.record ? this.record.UseAreaValueCH : undefined],
      fullAddressCH: [this.record ? this.record.fullAddressCH : undefined],
      DistrictId: [this.record ? this.record.DistrictId : undefined],
      UsageStatus: [this.record ? this.record.UsageStatus : undefined],
      Month: [60],
      Type: [1],
      TypeHouse: [this.record ? this.record.TypeHouse : undefined],
      ProcessProfileCeCode: [this.record ? this.record.ProcessProfileCeCode : undefined],

      memberRentFiles: [this.record ? this.record.memberRentFiles : [], []],
      WardId : [this.record ? this.record.WardId : undefined],
      LaneId : [this.record ? this.record.WardId : undefined],
      proviceId : [this.record ? this.record.WardId : undefined],
      CustomerName : [this.record ? this.record.CustomerName : undefined]
    });
    this.getDataCustomer();
    this.getDataBlock(true);
    this.getDataApartment();

    if (this.validateForm.value.RentBlockId) {
      this.GetBlockDataFilter(this.validateForm.value.RentBlockId);
    }

    if (this.validateForm.value.RentApartmentId) {
      this.GetApartmentDataFilter(this.validateForm.value.RentApartmentId);
    }
    console.log(this.record);
  }

  async submitForm() {
    this.loading = true;
    this.validateForm.get('memberRentFiles')?.setValue(this.MemberRentFileComponent.tableItemRef._data);
    this.validateForm.get('DistrictId')?.setValue(this.data_filter_block[0].District);
    this.validateForm.get('WardId')?.setValue(this.data_filter_block[0].Ward);
    if (this.apartment_data.length > 0) {
      this.validateForm.get('UsageStatus')?.setValue(this.apartment_data[0].UsageStatus);
    } else {
      this.validateForm.get('UsageStatus')?.setValue(this.data_filter_block[0].UsageStatus);
    }
    if (this.apartment_data.length > 0) {
      this.validateForm.get('TypeHouse')?.setValue(this.apartment_data[0].TypeHouse);
    } else {
      this.validateForm.get('TypeHouse')?.setValue(this.data_filter_block[0].TypeHouse);
    }
    let data = { ...this.validateForm.value };
    console.log(data);
    data.editHistory = this.editHistory;
    const resp = data.Id ? await this.rentFileRepository.update(data) : await this.rentFileRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  async getDataCustomer() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Code,FullName,Phone,Address,Dob';

    const resp = await this.customerRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.customer_data = [...resp.data];
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu Khách hàng!!!'
      });
    }
  }

  SetDataCustomer() {
    if (this.customer_data_filter) {
      this.validateForm.get('CodeKH')?.setValue(this.customer_data_filter[0].Code);
      this.validateForm.get('Phone')?.setValue(this.customer_data_filter[0].Phone);
      this.validateForm.get('Dob')?.setValue(convertDate(this.customer_data_filter[0].Dob));
      this.validateForm.get('AddressKH')?.setValue(this.customer_data_filter[0].Address);
      this.validateForm.get('CustomerName')?.setValue(this.customer_data_filter[0].FullName);
      const newArr: { Name: string; Relationship: string; Note: string; Check: boolean }[] = this.customer_data_filter.map(x => ({
        Name: x.FullName,
        Relationship: 'Đứng tên ký hợp đồng thuê nhà ở',
        Note: 'Không được sửa hoặc xóa!!!',
        Check: true
      }));
      let memberRentFiles = this.validateForm.value.memberRentFiles;
      if (memberRentFiles.length > 0) {
        for (let i = 0; i < memberRentFiles.length; i++) {
          if (memberRentFiles[i].Check == true) {
            memberRentFiles[i] = newArr[0];
            const removed = memberRentFiles.splice(i, 1);
            memberRentFiles.unshift(removed[0]);
            this.validateForm.get('memberRentFiles')?.setValue([...memberRentFiles]);
          }
        }
      } else {
        this.validateForm.get('memberRentFiles')?.setValue(newArr);
      }
    }
  }

  async GetCustomerFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    paging.select = 'Id,Code,FullName,Phone,Address,Dob';
    const resp = await this.customerRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.customer_data_filter = [...resp.data];
    }
    this.SetDataCustomer();
  }

  async getDataBlock(init?: boolean) {
    if (this.data_filter_block != undefined && !init) {
      this.validateForm.get('CampusArea')?.setValue(undefined);
      this.validateForm.get('fullAddressCN')?.setValue(undefined);
      this.validateForm.get('ConstructionAreaValue')?.setValue(undefined);
      this.validateForm.get('UseAreaValueCN')?.setValue(undefined);
      this.validateForm.get('CodeCN')?.setValue(undefined);
      this.validateForm.get('RentBlockId')?.setValue(undefined);
      this.block_Filter_data = [];
    }

    let TypeReportApply = this.validateForm.value.TypeReportApply;
    let TypeBlockId = this.validateForm.value.TypeBlockId;
    if (TypeReportApply && TypeBlockId) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `TypeReportApply=${TypeReportApply} AND TypeBlockId=${TypeBlockId} AND TypeBlockEntity=${2} `;
      paging.select = 'Id,Address,Lane,Ward,District,Province,Name';
      const resp = await this.blockRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.block_data = [...resp.data];
      }
    }
  }

  SetDataBlock() {
    if (this.data_filter_block) {
      console.log(this.data_filter_block);
      this.validateForm.get('CampusArea')?.setValue(this.data_filter_block[0].CampusArea);
      this.validateForm.get('fullAddressCN')?.setValue(this.data_filter_block[0].FullAddress);
      this.validateForm.get('ConstructionAreaValue')?.setValue(this.data_filter_block[0].ConstructionAreaValue);
      this.validateForm.get('UseAreaValueCN')?.setValue(this.data_filter_block[0].UseAreaValue);
      this.validateForm.get('CodeCN')?.setValue(this.data_filter_block[0].Code);
      this.validateForm.get('LaneId')?.setValue(this.data_filter_block[0].Lane);
      this.validateForm.get('proviceId')?.setValue(this.data_filter_block[0].Province);
    }
  }

  async GetBlockDataFilter(event: any) {
    if (event != undefined) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `Id=${event}`;
      const resp = await this.blockRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.block_Filter_data = [...resp.data[0].levelBlockMaps];
        this.data_filter_block = [...resp.data];
        console.log(this.data_filter_block);
        this.SetDataBlock();
      }
    }
  }

  SetDataApartment() {
    if (this.data_filter_apartment.length > 0) {
      this.validateForm.get('fullAddressCH')?.setValue(this.data_filter_apartment[0].Address);
      this.validateForm.get('UseAreaValueCH')?.setValue(this.data_filter_apartment[0].UseAreaValue);
      this.validateForm.get('CodeCH')?.setValue(this.data_filter_apartment[0].Code);
    }
  }

  async getDataApartment() {
    let BlockId = this.validateForm.value.RentBlockId;
    if (BlockId) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `BlockId=${BlockId}`;
      const resp = await this.apartmentRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.apartment_data = [...resp.data];
      }
    }
  }

  async GetApartmentDataFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    const resp = await this.apartmentRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.data_filter_apartment = [...resp.data];
      this.SetDataApartment();
    }
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2;
  };

  Addendum() {
    this.modalSrv.create({
      nzTitle: `Phụ lục cho hợp đồng  "${this.validateForm.value.Code}"  `,
      nzContent: AddendumComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record: undefined,
        parent: this.validateForm.value,
        typehouse_data: this.typehouse_data
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thêm phụ lục cho hợp đồng thành công!`);
        this.change = !this.change;
      }
    });
  }

  arrears() {
    const drawerRef = this.drawerService.create<ArrearsComponent>({
      nzTitle: `Phiếu truy thu cho hợp đồng "${this.validateForm.value.Code}"`,
      nzWidth: '85vw',
      nzContent: ArrearsComponent,
      nzPlacement: 'right',
      nzContentParams: {
        RentFileId: this.validateForm.value.Id
      }
    });
    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa phiếu truy thu thành công!` : `Thêm mới phiếu truy thu thành công!`;
        this.message.success(msg);
        this.change1 = !this.change1;
      }
    });
  }

  onClick() {
    this.rentFileRepository.GetExportWordPT09(this.record.Id);
  }

  onClick1() {
    this.rentFileRepository.GetExportWordPT10(this.record.Id);
  }

  async getProcessProfileCeData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;

    paging.select = 'Code';

    const resp = await this.processProfileCeRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.process_profile_ce_data = resp.data;
    }
  }
}
