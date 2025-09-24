import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { TypeHouse } from 'src/app/shared/utils/consts';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-noc3',
  templateUrl: './report-noc3.component.html',
  styles: []
})
export class ReportNoc3Component implements OnInit {
  datas: any;
  validateForm!: FormGroup;
  TypeHouse: { [key: number]: string } = TypeHouse;

  constructor(private reportRepository: ReportRepository, private fb: FormBuilder) {}
  ngOnInit(): void {
    this.validateForm = this.fb.group({
      DateStart: [new Date().toISOString().slice(0, 10)],
      DateEnd: [new Date().toISOString().slice(0, 10)]
    });
  }
  onClick() {
    this.getDataReport4();
  }

  async getDataReport4() {
    const resp = await this.reportRepository.ReportNOC3(this.validateForm.value.DateStart, this.validateForm.value.DateEnd);
    if (resp.meta?.error_code == 200) {
      this.datas = resp.data;
    }
  }

  genTypeHouseName(env: any): string {
    return this.TypeHouse[env];
  }

  selectDayStart() {
    const selectedDate = this.validateForm.value.DateStart;
  }
  selectDayEnd() {
    const selectedDate = this.validateForm.value.DateEnd;
  }

  async dowloadReportNOC3() {
    const resp = await this.reportRepository.ExportReportNOC3(
      this.datas,
      this.validateForm.value.DateStart,
      this.validateForm.value.DateEnd
    );
  }
}
