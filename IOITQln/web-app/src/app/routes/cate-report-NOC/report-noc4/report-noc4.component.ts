import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { UsageStatus } from 'src/app/shared/utils/consts';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-noc4',
  templateUrl: './report-noc4.component.html',
  styles: []
})
export class ReportNoc4Component implements OnInit {
  data: any;
  validateForm!: FormGroup;

  constructor(private reportRepository: ReportRepository, private fb: FormBuilder) {}
  UsageStatus: { [key: number]: string } = UsageStatus;

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
    const resp = await this.reportRepository.ReportNOC4(this.validateForm.value.DateStart, this.validateForm.value.DateEnd);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }

  selectDayStart() {
    const selectedDate = this.validateForm.value.DateStart;
  }
  selectDayEnd() {
    const selectedDate = this.validateForm.value.DateEnd;
  }

  async dowloadReportNOC4() {
    const resp = await this.reportRepository.ExportReportNOC4(
      this.data,
      this.validateForm.value.DateStart,
      this.validateForm.value.DateEnd
    );
  }
}
