import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ResettlementApartmentRepository } from 'src/app/infrastructure/repositories/resettlement-apartment.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { _ } from 'ajv';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-add-or-update-resettlement-apartment',
  templateUrl: './add-or-update-resettlement-apartment.component.html',
  styles: []
})
export class AddOrUpdateResettlementApartmentComponent implements OnInit {
  @ViewChild('tableRef') private tableItemRef!: STComponent;

  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() record: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private resettlementapartmentRepository: ResettlementApartmentRepository,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id
      ? await this.resettlementapartmentRepository.update(data)
      : await this.resettlementapartmentRepository.addNew(data);
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
