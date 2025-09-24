import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { ReportApartmentRepository } from 'src/app/infrastructure/repositories/tdc-report-apartment.repository';
import { AddOrUpdateApartmentReportComponent } from './add-or-update/add-or-update.component';  
import { TypeAttributeCode } from 'src/app/shared/utils/enums';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { TypeReception } from 'src/app/shared/utils/consts';
import { TypeOverYear } from 'src/app/shared/utils/consts';
import { ReportTdcRepository } from 'src/app/infrastructure/repositories/Report-document-add';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'app-tdc-report-apartment',
  templateUrl: './tdc-report-apartment.component.html',
  styles: [
  ]
})
export class TdcReportApartmentComponent implements OnInit {

  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  selectedTypeId: any;
  data: any[] = [];
  loading = false;
  typedecision_data: any[] = [];
  typelegal_data:any[] =[];
  project:any[]=[];
  district: any[]=[];
  columns: STColumn[] = [];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private reportApartmentRepository: ReportApartmentRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private typeAttributeRepository: TypeAttributeRepository,
    private tdcProjectRepository: TDCProjectRepository,
    private districtRepository: DistrictRepository,
    private reportTdcRepository:ReportTdcRepository,
    private notificationService: NzNotificationService
  ) {
    this.columns = [
      { title: 'Stt', type: 'no', width: 40 },
      { title: 'Tên dự án', index: 'TdcProjectName' },
      { title: 'Quyết định pháp lý', index: 'TypeLegalId', render:'typedecision-column' },
      { title: 'Thời gian tiếp nhận', index:'ReceptionDate', type: 'date', dateFormat: 'dd/MM/yyyy'},
      { title: 'Ghi chú', index: 'Note'},
      {
        title: 'Chức năng',
        width: 100,
        className: 'text-center',
        buttons:[
          {
            icon: 'edit',
            iif: i => !i.edit,
            click: record => this.addOrUpdate(record)
          },
          {
            icon: 'delete',
            type: 'del',
            pop: {
                title: 'Bạn có chắc chắn muốn xoá căn nhà này?',
                okType: 'danger',
                icon: 'star'
            },
            click: record => this.delete(record)
          }
        ]
      }
    ]
   }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getTypeDecisionData();
    this.getDataProjectTDC();
    this.getDistrictData();
    this.getTypeLegalData();
    // this.getDataDecreeType1();
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
      this.selectedTypeId = this.query.type;
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';
    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
        if (this.query.txtSearch.trim() != '')
            this.paging.query += `and (TdcProjectName.Contains("${this.query.txtSearch}")` + ` or TdcProjectName.Contains("${this.query.txtSearch}"))`;
    }

    if (this.query.type != undefined) {
      this.paging.query += ` and TypeLegalId=${this.query.type}`;
    }

    try {
        this.loading = true;
        const resp = await this.reportApartmentRepository.getByPage(this.paging);
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
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString())
    const drawerRef = this.drawerService.create<AddOrUpdateApartmentReportComponent>({
        nzTitle: record ? `Sửa căn hộ` : 'Thêm mới căn hộ',
        // record.khoa_chinh
        nzWidth: '75vw',
        nzContent: AddOrUpdateApartmentReportComponent,
        nzPlacement: 'left',
        nzContentParams: {
            record,
            typedecision_data: this.typedecision_data,
            typelegal_data: this.typelegal_data,
            
        }
    });

    drawerRef.afterClose.subscribe((data: any) => {
        if (data) {
            let msg = data.Id ? `Sửa danh sách căn hộ thành công!` : `Thêm mới căn hộ thành công!`;
            this.message.success(msg);
            this.getData();
        }
    });
  }

  async delete(data: any) {
    const resp = await this.reportApartmentRepository.delete(data);
    if (resp.meta?.error_code == 200) {
        this.message.create('success', `Xóa căn hộ thành công!`);
        this.getData();
    } else {
        this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  async getTypeDecisionData() {
    const resp = await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.LOAI_QUYET_DINH_TDC);

    if (resp.meta?.error_code == 200) {
        this.typedecision_data = resp.data;
    }
  }
  

  genTypeDecision(TypeLegalId: number) {
    let typedecision = this.typedecision_data.find(x => x.Id == TypeLegalId);

    return typedecision ? typedecision.Name : "";
  }

  async getTypeLegalData() {
    const resp = await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.LOAI_QUYET_DINH_TDC);

    if (resp.meta?.error_code == 200) {
        this.typelegal_data = resp.data;
    }
  }

  async getDataProjectTDC() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.tdcProjectRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.project = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }


  findTDCProject(id: number){
    let item = this.project.find(x => x.Id == id);
    return item ? item.Name : undefined;
  }

  async getDistrictData(){
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.districtRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.district = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không Lấy Được Dữ Liệu!!!'
      });
    }
  }

  async csvExport() {
    const resp = await this.reportApartmentRepository.ExportExcel();
  }

  async csvExport2() {
    const resp = await this.reportApartmentRepository.ExcelRP2(this.selectedTypeId);
  }

  async csvExport3() {
    const resp = await this.reportApartmentRepository.ExcelRP3();
  }

  async csvExport4() {
    const resp = await this.reportApartmentRepository.ExcelRP4();
  }

  async getDataRP3(){
    const resp = await this.reportTdcRepository.ReportTDC3();
    if (resp.meta?.error_code == 200) {
      this.data = resp.data; 
      this.showNotification('success', 'Tìm kiếm dữ liệu', 'Đã lấy được dữ liệu vui lòng xuất excel');
    }
  }

  showNotification(type: string, title: string, content: string): void {
    this.notificationService.create(type, title, content);
  }


  async downloadReport3(){
    const resp = await this.reportTdcRepository.ExportReportTDC3(this.data);
  }
  onBack() {
    window.history.back();
  }

}