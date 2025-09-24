import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';


@Component({
  selector: 'app-addorupdate',
  templateUrl: './addorupdate.component.html',
  styles: [
  ]
})
export class AddorupdateMd167HouseTypeComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
    private md167HouseTypeRepository: Md167HouseTypeRepository, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      // IsApplied: [this.record ? this.record.IsApplied : false, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined, []],
    });
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