import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { _HttpClient, SettingsService } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { zip } from 'rxjs';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { UserService } from 'src/app/core/services/user.service';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';

@Component({
  selector: 'app-account-changeinfo-base',
  templateUrl: './change-info.component.html',
  styleUrls: ['./change-info.component.less']
})
export class ChangeInfoComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    private cdr: ChangeDetectorRef,
    private msg: NzMessageService,
    private fb: FormBuilder,
    private userRepository: UserRepository,
    private userService: UserService,
    private uploadRepository: UploadRepository,
    private settings: SettingsService
  ) {}
  validateForm!: FormGroup;
  loading = false;
  srcImg = this.userService.getLoggedUser()['BaseUrlImg'];

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [undefined, []],
      UserName: [{ value: undefined, disabled: true }, [Validators.required, Validators.minLength(4)]],
      FullName: [undefined, [Validators.required, Validators.minLength(6)]],
      Phone: [undefined, [Validators.required, Validators.pattern(/(84|0[3|5|7|  9])+([0-9]{8})\b/g)]],
      Email: [undefined, [Validators.email]],
      Dob: [undefined, []],
      Address: [undefined, []],
      Avatar: [undefined, []]
    });

    this.getInfo();
  }

  async save() {
    let data = { ...this.validateForm.value };

    try {
      this.loading = true;
      let id = this.userService.getLoggedUser()['Id'];
      const resp = await this.userRepository.changeInfoUser(data, id);

      if (resp.meta?.error_code == 200) {
        this.msg.success(resp.meta?.error_message);
        let user = this.settings.user;
        let res = resp.data;

        user['FullName'] = res.FullName;
        user['Avatar'] = res.Avatar;

        this.settings.setUser(user);
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async getInfo() {
    try {
      let id = this.userService.getLoggedUser()['Id'];
      const resp = await this.userRepository.getAccountInfo(id);

      if (resp.meta?.error_code == 200) {
        let account = resp.data;
        this.validateForm.get('Id')?.setValue(account.Id);
        this.validateForm.get('UserName')?.setValue(account.UserName);
        this.validateForm.get('FullName')?.setValue(account.FullName);
        this.validateForm.get('Phone')?.setValue(account.Phone);
        this.validateForm.get('Email')?.setValue(account.Email);
        this.validateForm.get('Dob')?.setValue(convertDate(account.Dob));
        this.validateForm.get('Address')?.setValue(account.Address);
        this.validateForm.get('Avatar')?.setValue(account.Avatar);
      }
    } catch (error) {
      throw error;
    } finally {
    }
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadImage(formData);
    this.validateForm.get('Avatar')?.setValue(resp?.data.toString());
    this.cdr.detectChanges();
  }
}
