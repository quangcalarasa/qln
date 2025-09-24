import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { Md167HouseTypeRepository } from 'src/app/infrastructure/repositories/md167house-type.repository';
import { NzModalService } from 'ng-zorro-antd/modal';
import { ExtraEmailDebtRepository } from 'src/app/infrastructure/repositories/extra-email-debt.repository';

@Component({
  selector: 'app-email-notification',
  templateUrl: './email-notification.component.html',
  styles: [
  ]
})
export class EmailNotificationComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  dataBefore: any;
  record:any;

  constructor(private fb: FormBuilder,
    private modalSrv: NzModalService,
    private extraEmailDebtRepository: ExtraEmailDebtRepository,

  ) {
    this.getData()

   }

  ngOnInit(): void {
    
    this.validateForm = this.fb.group({
      HostSent: [undefined, [Validators.required]],
      EmailSent: [undefined, [Validators.required]],
      SenderName: [undefined, [Validators.required]],
      PassHashSent: [undefined, [Validators.required]],
      PortSentEmail: [undefined, [Validators.required]],
    });
  }
  async getData() {
      const resp = await this.extraEmailDebtRepository.getPort();

      if (resp.meta?.error_code == 200) {
        this.validateForm.get('HostSent')?.setValue(resp.data.HostSent)
        this.validateForm.get('EmailSent')?.setValue(resp.data.EmailSent)
        this.validateForm.get('SenderName')?.setValue(resp.data.SenderName)
        this.validateForm.get('PassHashSent')?.setValue(resp.data.PassHashSent)
        this.validateForm.get('PortSentEmail')?.setValue(resp.data.PortSentEmail)
        this.record=resp.data
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu.'
        });
      }
  }
  clear() {
    this.validateForm = this.fb.group({
      HostSent: [this.record ? this.record.HostSent : undefined, [Validators.required]],
      EmailSent: [this.record ? this.record.EmailSent : undefined, [Validators.required]],
      SenderName: [this.record ? this.record.SenderName : undefined, [Validators.required]],
      PassHashSent: [this.record ? this.record.PassHashSent : undefined, [Validators.required]],
      PortSentEmail: [this.record ? this.record.PortSentEmail : undefined, [Validators.required]],
    });
  }
  submitForm()
  {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };

    const resp = this.extraEmailDebtRepository.updatePort(data)
    this.loading = false;
    this.modalSrv.info({
      nzTitle: ' Cập nhật thành công'
    });
  }
}
