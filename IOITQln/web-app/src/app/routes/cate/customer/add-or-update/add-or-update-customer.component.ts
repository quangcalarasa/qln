import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import * as CryptoJS from 'crypto-js';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { CustomerRepository } from 'src/app/infrastructure/repositories/customer.repository';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { CommonService } from 'src/app/core/services/common.service';
import { AccessKey } from 'src/app/shared/utils/enums';

@Component({
  selector: 'app-add-or-update-customer',
  templateUrl: './add-or-update-customer.component.html'
})
export class AddOrUpdateCustomerComponent implements OnInit {
  @Input() record: NzSafeAny;
  @Input() srcImg?: string;
  nzFormat = 'dd/ MM/ yyyy';
  validateForm!: FormGroup;
  loading: boolean = false;
  fileList: NzUploadFile[] = [];
  role = this.commonService.CheckAccessKeyRole(AccessKey.CUSTOMER_MANAGEMENT);

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private customerRepository: CustomerRepository,
    private message: NzMessageService,
    private modalSrv: NzModalService,
    private uploadRepository: UploadRepository,
    private commonService: CommonService,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined, []],
      Code: [this.record ? this.record.Code : undefined, [Validators.required, Validators.minLength(9)]],
      FullName: [this.record ? this.record.FullName : undefined, [Validators.required, Validators.minLength(6)]],
      Phone: [this.record ? this.record.Phone : undefined, [Validators.required, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)]],
      Email: [this.record ? this.record.Email : undefined, [Validators.email]],
      Dob: [this.record ? convertDate(this.record.Dob) : undefined, []],
      Address: [this.record ? this.record.Address : undefined, []],
      Sex: [this.record ? (this.record.Sex ? this.record.Sex.toString() : undefined) : "1", []],
      Avatar: [this.record ? this.record.Avatar : undefined, []],
      Doc: [this.record ? convertDate(this.record.Doc) : undefined, []],
      PlaceCode: [this.record ? this.record.PlaceCode : undefined, []]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    data.IsRoleGroup = true;

    if (!data.Id && data.Password) {
      data.Password = CryptoJS.MD5(data.Password).toString();
    }

    const resp = data.Id ? await this.customerRepository.update(data) : await this.customerRepository.addNew(data);

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

  compareFn = (o1: any, o2: any) => (o1 && o2 ? o1.RoleId === o2.RoleId : o1 === o2);

  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadImage(formData);
    this.validateForm.get('Avatar')?.setValue(resp?.data.toString());
  }
}
