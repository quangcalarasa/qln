import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { UsageStatus } from 'src/app/shared/utils/consts';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-noc5',
  templateUrl: './report-noc5.component.html',
  styles: []
})
export class ReportNOC5Component implements OnInit {
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
    this.getDataReport5();
  }

  async getDataReport5() {
    const resp = await this.reportRepository.getReport5(this.validateForm.value.DateStart, this.validateForm.value.DateEnd);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }
  genUsageStatusName(env: any): string {
    return this.UsageStatus[env];
  }

  async dowloadReportNOC5() {
    const resp = await this.reportRepository.ExportReportNOC5(
      this.data,
      this.validateForm.value.DateStart,
      this.validateForm.value.DateEnd
    );
  }

  selectDayStart() {
    const selectedDate = this.validateForm.value.DateStart;
  }
  selectDayEnd() {
    const selectedDate = this.validateForm.value.DateEnd;
  }
}
