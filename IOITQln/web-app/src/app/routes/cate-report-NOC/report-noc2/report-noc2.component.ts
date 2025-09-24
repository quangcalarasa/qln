import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { TypeHouse } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import QueryModel from 'src/app/core/models/query-model';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-noc2',
  templateUrl: './report-noc2.component.html',
  styles: []
})
export class ReportNoc2Component implements OnInit {
  data: any;
  typehouse_data: any;
  TypeHouse: { [key: number]: string } = TypeHouse;
  query: QueryModel = new QueryModel();
  validateForm!: FormGroup;

  constructor(private reportRepository: ReportRepository, private typeBlockRepository: TypeBlockRepository, private fb: FormBuilder) {
    this.getTypeBlockData();
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      TypeBlockId: [undefined]
    });
  }

  onClick() {
    this.getDataReport4();
  }

  async getDataReport4() {
    const resp = await this.reportRepository.ReportNOC2(this.validateForm.value.TypeBlockId);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }

  genTypeHouseName(env: any): string {
    return this.TypeHouse[env];
  }

  async getTypeBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.typeBlockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    }
  }

  async dowloadReportNOC2() {
    const resp = await this.reportRepository.ExportReportNOC2(this.data);
  }
}
