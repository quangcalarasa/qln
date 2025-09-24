import { Component, OnInit } from '@angular/core';
import { ReportRepository } from 'src/app/infrastructure/repositories/Report.repository';
import { UsageStatus } from 'src/app/shared/utils/consts';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-noc6',
  templateUrl: './report-noc6.component.html',
  styles: []
})
export class ReportNoc6Component implements OnInit {
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
    this.ReportNOC6();
  }

  async ReportNOC6() {
    const resp = await this.reportRepository.ReportNOC6(this.validateForm.value.DateStart, this.validateForm.value.DateEnd);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }
  genUsageStatusName(env: any): string {
    return this.UsageStatus[env];
  }

  async ExportReportNOC6() {
    const resp = await this.reportRepository.ExportReportNOC6(this.data);
  }

  selectDayStart() {
    const selectedDate = this.validateForm.value.DateStart;
  }
  selectDayEnd() {
    const selectedDate = this.validateForm.value.DateEnd;
  }
}
