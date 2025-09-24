import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167AuctioneerRepository } from 'src/app/infrastructure/repositories/md167auctioneer.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UserService } from 'src/app/core/services/user.service';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';


@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateMd167AuctioneerComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  TypeQD = TypeQD;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private uploadRepository: UploadRepository,
    private md167AuctioneerRepository: Md167AuctioneerRepository,
    private cdr: ChangeDetectorRef,
    private userService: UserService
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      UnitName: [this.record ? this.record.UnitName : undefined, [Validators.required]],
      UnitAddress: [this.record ? this.record.UnitAddress : undefined, [Validators.required]],
      AutInfo: [this.record ? this.record.AutInfo : undefined, []],
      TaxNumber: [this.record ? this.record.TaxNumber : undefined, [Validators.required]],
      BusinessLicense: [this.record ? this.record.BusinessLicense : undefined, [Validators.required]],
      RepresentFullName: [this.record ? this.record.RepresentFullName : undefined, [Validators.required]],
      RepresentPosition: [this.record ? this.record.RepresentPosition : undefined, [Validators.required]],
      RepresentIDCard: [this.record ? this.record.RepresentIDCard : undefined, []],
      RepresentDateOfIssue: [this.record ? convertDate(this.record.RepresentDateOfIssue) : undefined, []],
      RepresentPlaceOfIssue: [this.record ? this.record.RepresentPlaceOfIssue : undefined, []],
      ContactAddress: [this.record ? this.record.ContactAddress : undefined, [Validators.required]],
      ContactPhoneNumber: [this.record ? this.record.ContactPhoneNumber : undefined, [Validators.required]],
    });
  }


  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";

    const resp = data.Id ? await this.md167AuctioneerRepository.update(data) : await this.md167AuctioneerRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    }
    else {
      this.loading = false;
    }
  }
  beforeUpload = (file: NzUploadFile): boolean => {
    console.log(file);
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadFile(formData);
    this.validateForm.get('AutInfo')?.setValue(resp?.data.toString());

  }

  close(): void {
    this.drawerRef.close();
  }

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }
}

