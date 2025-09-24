import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import QueryModel from 'src/app/core/models/query-model';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { ReportTdcRepository } from 'src/app/infrastructure/repositories/Report-document-add';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { TypeAttributeCode } from 'src/app/shared/utils/enums';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'app-report2-all',
  templateUrl: './report2-all.component.html',
  styles: [
  ]
})
export class Report2AllComponent implements OnInit {
  data: any;
  typelegal_data: any;
  selectedTypeId: any;
  query: QueryModel = new QueryModel();
  validateForm!: FormGroup;

  constructor(
    private reportTdcRepository:ReportTdcRepository, 
    private typeAttributeRepository: TypeAttributeRepository, 
    private fb: FormBuilder, 
    private notificationService: NzNotificationService) {
    this.getTypeAttributeData();
   }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      TypeLegalId: [undefined]
    });
  }

  async getTypeAttributeData(){
    const resp =await this.typeAttributeRepository.getItemByTypeAttributeCode(TypeAttributeCode.LOAI_QUYET_DINH_TDC);

    if (resp.meta?.error_code == 200) {
      this.typelegal_data = resp.data;
      
    }
  }
  onClick() {
    this.getDataReport2();
    this.showNotification('success', 'Tìm kiếm dữ liệu', 'Đã lấy được dữ liệu vui lòng xuất excel');
  }

  showNotification(type: string, title: string, content: string): void {
    this.notificationService.create(type, title, content);
  }

  async getDataReport2(){
    const resp = await this.reportTdcRepository.ReportTDC2(this.validateForm.value.TypeLegalId);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data; 
    }
  }

 
  async downloadReport2(){
    const resp = await this.reportTdcRepository.ExportReportTDC2(this.data);
  }

}
