import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';
import { UserService } from 'src/app/core/services/user.service';
import * as CryptoJS from 'crypto-js';
import { NzMessageService } from 'ng-zorro-antd/message';
import { rgxPassword, errMsgRgxPassword } from 'src/app/shared/utils/consts';

@Component({
  selector: 'app-account-changepass-base',
  templateUrl: './change-pass.component.html',
  styleUrls: ['./change-pass.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangePassComponent implements OnInit {
  constructor(
    private fb: FormBuilder,
    private userRepository: UserRepository,
    private userService: UserService,
    private cdr: ChangeDetectorRef,
    private msg: NzMessageService
  ) { }
  validateForm!: FormGroup;
  loading = false;
  srcImg = this.userService.getLoggedUser()['BaseUrlImg'];
  rgxPassword = rgxPassword;
  errMsgRgxPassword = errMsgRgxPassword;

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      CurrentPassword: [undefined, [Validators.required]],
      NewPassword: [undefined, [Validators.required, Validators.pattern(rgxPassword)]],
      ConfirmPassword: [undefined, this.confirmValidator]
    });
  }

  confirmValidator = (control: FormControl): { [s: string]: boolean } => {
    if (!control.value) {
      return { error: true, required: true };
    } else if (control.value !== this.validateForm.value.NewPassword) {
      return { confirm: true, error: true };
    }
    return {};
  };

  validateConfirmPassword(): void {
    setTimeout(() => {
      if (this.validateForm.value.NewPassword) this.validateForm.value.NewPassword.updateValueAndValidity();
    });
  }

  async save() {
    let data = { ...this.validateForm.value };

    // if (data.CurrentPassword) {
    //   data.CurrentPassword = CryptoJS.MD5(data.CurrentPassword).toString();
    // }

    // if (data.NewPassword) {
    //   data.NewPassword = CryptoJS.MD5(data.NewPassword).toString();
    // }

    delete data['ConfirmPassword'];

    try {
      this.loading = true;
      let id = this.userService.getLoggedUser()['Id'];
      const resp = await this.userRepository.changePassApplyKeycloak(data, id);

      if (resp.meta?.error_code == 200) {
        this.msg.success(resp.meta?.error_message);
        window.history.back();
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }
}
