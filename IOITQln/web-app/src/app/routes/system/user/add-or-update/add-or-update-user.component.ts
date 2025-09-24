import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import * as CryptoJS from 'crypto-js';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { NzModalService } from 'ng-zorro-antd/modal';
import { RoleRepository } from 'src/app/infrastructure/repositories/role.repository';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { rgxPassword, errMsgRgxPassword } from 'src/app/shared/utils/consts';
import { SharedTreeViewCheckboxComponent } from 'src/app/shared/components/tree-view-checkbox/tree-view-checkbox.component';

@Component({
  selector: 'app-add-or-update-user',
  templateUrl: './add-or-update-user.component.html'
})
export class AddOrUpdateUserComponent implements OnInit {
  @ViewChild('treeViewCheckbox') treeViewCheckbox!: SharedTreeViewCheckboxComponent;

  @Input() record: NzSafeAny;
  @Input() srcImg?: string;
  @Input() districts: any[] = [];
  @Input() wards: any[] = [];

  nzScrollYValue = window.innerHeight - 450 + 'px';
  validateForm!: FormGroup;
  loading: boolean = false;
  fileList: NzUploadFile[] = [];
  rolesInput: any[] = [];
  rgxPassword = rgxPassword;
  errMsgRgxPassword = errMsgRgxPassword;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private userRepository: UserRepository,
    private message: NzMessageService,
    private modalSrv: NzModalService,
    private roleRepository: RoleRepository,
    private uploadRepository: UploadRepository
  ) { }

  ngOnInit(): void {
    this.getListRole();

    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined, []],
      UserName: [
        { value: this.record ? this.record.UserName : undefined, disabled: this.record ? true : false },
        [Validators.required, Validators.minLength(4)]
      ],
      Password: [this.record ? this.record.Password : undefined, !this.record ? [Validators.required, Validators.pattern(rgxPassword)] : []],
      ConfirmPassword: [this.record ? this.record.Password : undefined, !this.record ? this.confirmValidator : []],
      FullName: [this.record ? this.record.FullName : undefined, [Validators.required, Validators.minLength(6)]],
      Phone: [this.record ? this.record.Phone : undefined, [Validators.required, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)]],
      Email: [this.record ? this.record.Email : undefined, [Validators.required, Validators.email]],
      Dob: [this.record ? convertDate(this.record.Dob) : undefined, []],
      Address: [this.record ? this.record.Address : undefined, []],
      Avatar: [this.record ? this.record.Avatar : undefined, []],
      ModuleSystem: [this.record ? (this.record.ModuleSystem ? this.record.ModuleSystem.toString() : undefined) : undefined, []],
      userRoles: [this.record ? (this.record.userRoles.length == 0 ? undefined : this.record.userRoles) : undefined, [Validators.required]],
      wardManagements: [this.record ? (this.record.wardManagements.length == 0 ? undefined : this.record.wardManagements) : undefined, []]
    });
  }

  confirmValidator = (control: FormControl): { [s: string]: boolean } => {
    if (!control.value) {
      return { error: true, required: true };
    } else if (control.value !== this.validateForm.value.Password) {
      return { confirm: true, error: true };
    }
    return {};
  };

  validateConfirmPassword(): void {
    setTimeout(() => {
      if (this.validateForm.value.PassWord) this.validateForm.value.PassWord.updateValueAndValidity();
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.IsRoleGroup = true;

    // if (!data.Id && data.Password) {
    //   data.Password = CryptoJS.MD5(data.Password).toString();
    // }

    data.wardManagements = this.treeViewCheckbox ? this.treeViewCheckbox.getDataSelected() : undefined;

    const resp = data.Id ? await this.userRepository.update(data) : await this.userRepository.addNew(data);

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

  async getListRole() {
    const paging: GetByPageModel = new GetByPageModel();
    paging.select = 'Id,Name,Note';
    paging.order_by = 'Name Asc';
    paging.page_size = -1;

    const resp = await this.roleRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      // this.rolesInput = resp.data;
      this.rolesInput = resp.data.map((x: any) => {
        return { RoleId: x.Id, RoleName: x.Name, FullName: x.Name + (x.Note ? ` (${x.Note})` : ``) };
      });
    }
  }
}
