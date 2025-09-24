import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Decree } from 'src/app/shared/utils/consts';
import { SalaryRepository } from 'src/app/infrastructure/repositories/salary.repository';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-add-or-update-salary',
  templateUrl: './add-or-update-salary.component.html',
  styles: []
})
export class AddOrUpdateSalaryComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;
  @Input() decree_type2_data: NzSafeAny;
  nzFormat = 'dd/ MM/ yyyy';
  decree_type1_data = Decree;
  role = this.commonService.CheckAccessKeyRole(AccessKey.SALARY_MANAGEMENT);
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private salaryRepository: SalaryRepository,
    private cdr: ChangeDetectorRef,
    private commonService: CommonService,
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      DecreeType1Id: [this.record ? (this.record.DecreeType1Id ? this.record.DecreeType1Id.toString() : undefined) : undefined, []],
      DecreeType2Id: [this.record ? this.record.DecreeType2Id : undefined, [Validators.required]],
      DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, [Validators.required]],
      Value: [this.record ? this.record.Value : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined, []]
    });
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.salaryRepository.update(data) : await this.salaryRepository.addNew(data);
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
}
