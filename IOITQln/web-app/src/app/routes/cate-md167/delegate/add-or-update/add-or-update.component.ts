import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Md167DelegateRepository } from 'src/app/infrastructure/repositories/md167delegate.repository';
import { TypeQD } from 'src/app/shared/utils/consts';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { UserService } from 'src/app/core/services/user.service';


@Component({
  selector: 'app-add-or-update',
  templateUrl: './add-or-update.component.html',
  styles: [
  ]
})
export class AddOrUpdateDelegateComponent implements OnInit {
  validateForm!: FormGroup;
  loading: boolean = false;
  dataPer: any;
  dataCom: any;
  @Input() record: NzSafeAny;

  constructor(private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private md167DelegateRepository: Md167DelegateRepository,
    private cdr: ChangeDetectorRef,
    private uploadRepository: UploadRepository,
    private userService: UserService
  ) {
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code: [{ value: this.record ? this.record.Code : undefined, disabled: true }, []],
      PersonOrCompany: [this.record ? this.record.PersonOrCompany : 1, [Validators.required]],
      Name: [this.record ? this.record.Name : undefined, [Validators.required]],
      NationalId: [this.record ? this.record.NationalId : undefined, []],
      DateOfIssue: [this.record ? convertDate(this.record.DateOfIssue) : undefined, []],
      PlaceOfIssue: [this.record ? this.record.PlaceOfIssue : undefined, []],
      AutInfo: [this.record ? this.record.AutInfo : undefined, []],
      Address: [this.record ? this.record.Address : undefined, [Validators.required]],
      PhoneNumber: [this.record ? this.record.PhoneNumber : undefined, [Validators.required]],
      ComTaxNumber: [this.record ? this.record.ComTaxNumber : undefined, []],
      ComBusinessLicense: [this.record ? this.record.ComBusinessLicense : undefined, []],
      ComOrganizationRepresentativeName: [this.record ? this.record.ComOrganizationRepresentativeName : undefined, []],
      ComPosition: [this.record ? this.record.ComPosition : undefined, []],
    });
    this.getDataDefault()

  }
  change() {
    this.validateForm.value.PersonOrCompany == 1 ? this.validateForm.get('PersonOrCompany')?.setValue(2) : this.validateForm.get('PersonOrCompany')?.setValue(1);
    if (this.validateForm.value.PersonOrCompany == 1) {

      this.dataCom.Name = this.validateForm.value.Name;
      this.dataCom.NationalId = this.validateForm.value.NationalId;
      this.dataCom.DateOfIssue = this.validateForm.value.DateOfIssue;
      this.dataCom.PlaceOfIssue = this.validateForm.value.PlaceOfIssue;
      this.dataCom.Address = this.validateForm.value.Address;
      this.dataCom.PhoneNumber = this.validateForm.value.PhoneNumber;
      this.dataCom.ComTaxNumber = this.validateForm.value.ComTaxNumber;
      this.dataCom.ComBusinessLicense = this.validateForm.value.ComBusinessLicense;
      this.dataCom.ComOrganizationRepresentativeName = this.validateForm.value.ComOrganizationRepresentativeName;
      this.dataCom.ComPosition = this.validateForm.value.ComPosition;

      this.validateForm.get("Name")?.setValue(this.dataPer.Name);
      this.validateForm.get("NationalId")?.setValue(this.dataPer.NationalId);
      this.validateForm.get("DateOfIssue")?.setValue(this.dataPer.DateOfIssue);
      this.validateForm.get("PlaceOfIssue")?.setValue(this.dataPer.PlaceOfIssue);
      this.validateForm.get("Address")?.setValue(this.dataPer.Address);
      this.validateForm.get("PhoneNumber")?.setValue(this.dataPer.PhoneNumber);
      this.validateForm.get("ComTaxNumber")?.setValue(this.dataPer.ComTaxNumber);
      this.validateForm.get("ComBusinessLicense")?.setValue(this.dataPer.ComBusinessLicense);
      this.validateForm.get("ComOrganizationRepresentativeName")?.setValue(this.dataPer.ComOrganizationRepresentativeName);
      this.validateForm.get("ComPosition")?.setValue(this.dataPer.ComPosition);

      this.validateForm.get("ComTaxNumber")?.setValidators(null);
      this.validateForm.get("ComBusinessLicense")?.setValidators(null);
      this.validateForm.get("ComOrganizationRepresentativeName")?.setValidators(null);
      this.validateForm.get("ComPosition")?.setValidators(null);

    }
    else {

      this.dataPer.Name = this.validateForm.value.Name;
      this.dataPer.NationalId = this.validateForm.value.NationalId;
      this.dataPer.DateOfIssue = this.validateForm.value.DateOfIssue;
      this.dataPer.PlaceOfIssue = this.validateForm.value.PlaceOfIssue;
      this.dataPer.Address = this.validateForm.value.Address;
      this.dataPer.PhoneNumber = this.validateForm.value.PhoneNumber;
      this.dataPer.ComTaxNumber = this.validateForm.value.ComTaxNumber;
      this.dataPer.ComBusinessLicense = this.validateForm.value.ComBusinessLicense;
      this.dataPer.ComOrganizationRepresentativeName = this.validateForm.value.ComOrganizationRepresentativeName;
      this.dataPer.ComPosition = this.validateForm.value.ComPosition;

      this.validateForm.get("Name")?.setValue(this.dataCom.Name);
      this.validateForm.get("NationalId")?.setValue(this.dataCom.NationalId);
      this.validateForm.get("DateOfIssue")?.setValue(this.dataCom.DateOfIssue);
      this.validateForm.get("PlaceOfIssue")?.setValue(this.dataCom.PlaceOfIssue);
      this.validateForm.get("Address")?.setValue(this.dataCom.Address);
      this.validateForm.get("PhoneNumber")?.setValue(this.dataCom.PhoneNumber);
      this.validateForm.get("ComTaxNumber")?.setValue(this.dataCom.ComTaxNumber);
      this.validateForm.get("ComBusinessLicense")?.setValue(this.dataCom.ComBusinessLicense);
      this.validateForm.get("ComOrganizationRepresentativeName")?.setValue(this.dataCom.ComOrganizationRepresentativeName);
      this.validateForm.get("ComPosition")?.setValue(this.dataCom.ComPosition);

      this.validateForm.get("ComTaxNumber")?.setValidators([Validators.required]);
      this.validateForm.get("ComBusinessLicense")?.setValidators([Validators.required]);
      this.validateForm.get("ComOrganizationRepresentativeName")?.setValidators([Validators.required]);
      this.validateForm.get("ComPosition")?.setValidators([Validators.required]);
    }
    this.validateForm.get('ComTaxNumber')?.updateValueAndValidity();
    this.validateForm.get('ComBusinessLicense')?.updateValueAndValidity();
    this.validateForm.get('ComOrganizationRepresentativeName')?.updateValueAndValidity();
    this.validateForm.get('ComPosition')?.updateValueAndValidity();
  }
  getDataDefault() {
    const dataB = {
      Name: undefined,
      NationalId: undefined,
      DateOfIssue: undefined,
      PlaceOfIssue: undefined,
      Address: undefined,
      PhoneNumber: undefined,
      ComTaxNumber: undefined,
      ComBusinessLicense: undefined,
      ComOrganizationRepresentativeName: undefined,
      ComPosition: undefined,
    }
    if (this.record) {
      const dataA = {
        Name: this.record.Name,
        NationalId: this.record.NationalId,
        DateOfIssue: this.record.DateOfIssue,
        PlaceOfIssue: this.record.PlaceOfIssue,
        Address: this.record.Address,
        PhoneNumber: this.record.PhoneNumber,
        ComTaxNumber: this.record.ComTaxNumber,
        ComBusinessLicense: this.record.ComBusinessLicense,
        ComOrganizationRepresentativeName: this.record.ComOrganizationRepresentativeName,
        ComPosition: this.record.ComPosition,
      }
      this.dataPer = this.record.PersonOrCompany === 1 ? dataA : dataB;
      this.dataCom = this.record.PersonOrCompany === 1 ? dataB : dataA;
      if (this.validateForm.value.PersonOrCompany === 2) {
        this.validateForm.get("ComTaxNumber")?.setValidators([Validators.required]);
        this.validateForm.get("ComBusinessLicense")?.setValidators([Validators.required]);
        this.validateForm.get("ComOrganizationRepresentativeName")?.setValidators([Validators.required]);
        this.validateForm.get("ComPosition")?.setValidators([Validators.required]);
        this.cdr.detectChanges();
        this.validateForm.updateValueAndValidity();

      }
    }
    else {
      this.dataPer = {
        Name: undefined,
        NationalId: undefined,
        DateOfIssue: undefined,
        PlaceOfIssue: undefined,
        Address: undefined,
        PhoneNumber: undefined,
        ComTaxNumber: undefined,
        ComBusinessLicense: undefined,
        ComOrganizationRepresentativeName: undefined,
        ComPosition: undefined,
      }
      this.dataCom = {
        Name: undefined,
        NationalId: undefined,
        DateOfIssue: undefined,
        PlaceOfIssue: undefined,
        Address: undefined,
        PhoneNumber: undefined,
        ComTaxNumber: undefined,
        ComBusinessLicense: undefined,
        ComOrganizationRepresentativeName: undefined,
        ComPosition: undefined,
      }
    }
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    data.Code = "";

    const resp = data.Id ? await this.md167DelegateRepository.update(data) : await this.md167DelegateRepository.addNew(data);
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

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }
}

