import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';
import { ExtraEmailDebtRepository } from 'src/app/infrastructure/repositories/extra-email-debt.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { ExtraTemplateRepository } from 'src/app/infrastructure/repositories/extra-template.repository';
import { NzModalService } from 'ng-zorro-antd/modal';


@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateEmaiDebtComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  template: any;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private extraEmailDebtRepository: ExtraEmailDebtRepository,
    private extraTemplateRepository: ExtraTemplateRepository,
    private modalSrv: NzModalService,
     private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.getDataTemplate()
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [this.record ? this.record.Code : undefined, [Validators.required]],
      TemplateId: [this.record ? this.record.TemplateId : undefined, [Validators.required]],
      Header: [this.record ? this.record.Header : undefined, [Validators.required]],
      IsAuto: [this.record ? this.record.IsAuto : true, [Validators.required]],
    });
  }
  change() {
    this.validateForm.get('IsAuto')?.setValue(!this.validateForm.value.IsAuto);
  }
  async getDataTemplate() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.extraTemplateRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.template = resp.data;
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
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = data.Id ? await this.extraEmailDebtRepository.update(data) : await this.extraEmailDebtRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }

  close(): void {
    this.drawerRef.close();
  }
}