import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';


@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateEmailSentComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167HouseTypeRepository: Md167HouseTypeRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      EmailHeader: [{ value: this.record ? this.record.EmailHeader : undefined, disabled: true }, []],
      ContentEmail: [{ value: this.record ? this.record.ContentEmail : undefined, disabled: true }, []],
      DateEmail: [{ value: this.record ? convertDate(this.record.DateEmail) : undefined, disabled: true }, []],
    });
  }
  change()
  {
    this.validateForm.get('IsAuto')?.setValue(!this.validateForm.value.IsAuto);
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";

    const resp = data.Id ? await this.md167HouseTypeRepository.update(data) : await this.md167HouseTypeRepository.addNew(data);
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
