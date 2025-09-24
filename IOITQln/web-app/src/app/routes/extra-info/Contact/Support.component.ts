import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { _HttpClient, SettingsService } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { UserService } from 'src/app/core/services/user.service';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { ContactRepository } from 'src/app/infrastructure/repositories/contact.repositorty';


@Component({
  selector: 'app-account-changeinfo-base',
  templateUrl: './Support.component.html',
})

export class SupportComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    private cdr: ChangeDetectorRef,
    private msg: NzMessageService,
    private fb: FormBuilder,
    private userRepository: UserRepository,
    private userService: UserService,
    private uploadRepository: UploadRepository,
    private settings: SettingsService,
    private ContactRepository : ContactRepository,
    private message: NzMessageService
  ) {}
  validateForm!: FormGroup;
  loading = false;
  srcImg = this.userService.getLoggedUser()['BaseUrlImg'];

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [undefined, []],
      FullName: [undefined, [Validators.required, Validators.minLength(6)]],
      Phone: [undefined, [Validators.required, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)]],
      Email: [undefined, [Validators.email]],
      State : [1],
      SupportType : [undefined , []],
      Content : [undefined,[]],
      File : [undefined, []],
      Code : [undefined, [],]
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ContactRepository.update(data) : await this.ContactRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.validateForm.reset('Id');
      this.validateForm.reset('FullName');
      this.validateForm.reset('Phone');
      this.validateForm.reset('Email');
      this.validateForm.reset('State');
      this.validateForm.reset('SupportType');
      this.validateForm.reset('Content');
      this.validateForm.reset('File');
      this.validateForm.reset('Code');
      this.message.create('success', `Gửi yêu cầu hỗ trợ thành công!`);
    } else {
      this.message.create('error', `Gửi yêu cầu hỗ trợ thất bại!`);
      this.loading = false;
    }
  }

  onBack() {
    window.history.back();
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadFile(formData);
    this.validateForm.get('File')?.setValue(resp?.data.toString());
  }

  removeAttactment() {
    this.validateForm.get('File')?.setValue(undefined);
}
}
