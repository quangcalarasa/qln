import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167PositionValueRepository } from 'src/app/infrastructure/repositories/md167positionvalue.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { DecreeMd167 } from 'src/app/shared/utils/consts';

@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdatePositionValueComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;
  // date = new Date();
  // nzMode = 'day';
  nzFormat = 'dd/ MM/ yyyy';
  lstDecree = DecreeMd167;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167PositionValueRepository: Md167PositionValueRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      decree: [this.record ? this.record.decree : undefined, [Validators.required]],
      DoApply: [this.record ? convertDate(this.record.DoApply) : undefined, [Validators.required]],
      Text1: [this.record ? this.record.Text1 : undefined],
      Text2: [this.record ? this.record.Text2 : undefined],
      Text3: [this.record ? this.record.Text3 : undefined],
      Text4: [this.record ? this.record.Text4 : undefined],
      Position1: [this.record ? this.record.Position1 : undefined, [Validators.required]],
      Position2: [this.record ? this.record.Position2 : undefined, [Validators.required]],
      Position3: [this.record ? this.record.Position3 : undefined, [Validators.required]],
      Position4: [this.record ? this.record.Position4 : undefined, [Validators.required]],
    });
  }


  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";

    const resp = data.Id ? await this.md167PositionValueRepository.update(data) : await this.md167PositionValueRepository.addNew(data);
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

