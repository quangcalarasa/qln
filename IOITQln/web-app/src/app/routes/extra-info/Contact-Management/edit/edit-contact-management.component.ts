import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzMessageService } from 'ng-zorro-antd/message';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { ContactRepository } from 'src/app/infrastructure/repositories/contact.repositorty';


@Component({
  selector: 'extra-info/Contact-Management/edit',
  templateUrl: './edit-contact-management.component.html',
  styles: []
})
export class EditContactManageComponent implements OnInit {
  @Input() record: NzSafeAny;
  validateForm!: FormGroup;
  loading: boolean = false;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private uploadRepository: UploadRepository,
    private ContactRepository : ContactRepository

  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      FullName: [this.record ? this.record.FullName : undefined, [Validators.required, Validators.minLength(6)]],
      Phone: [this.record ? this.record.Phone : undefined, [Validators.required, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)]],
      Email: [this.record ? this.record.Email : undefined, [Validators.email]],
      State : [this.record ? (this.record.State ? this.record.State.toString(): undefined )  : undefined, []],
      SupportType : [this.record ? (this.record.SupportType ? this.record.SupportType.toString() : undefined) : undefined , []],
      Content : [this.record ? this.record.Content : undefined,[]],
      File : [this.record ? this.record.File : undefined, []],
      Code : [this.record ? this.record.Code : undefined, [],]
    });
    console.log(this.record);
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ContactRepository.update(data) : await this.ContactRepository.addNew(data);
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

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
}
}
