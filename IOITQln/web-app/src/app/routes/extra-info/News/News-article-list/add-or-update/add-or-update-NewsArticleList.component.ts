import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ExtraNewsArticleListRepository } from 'src/app/infrastructure/repositories/ExtraNewsArticleList.repositories';

import { UserService } from 'src/app/core/services/user.service';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';


@Component({
  selector: 'extra-info/News/News-article-list/add-or-update',
  templateUrl: './add-or-update-NewsArticleList.component.html',
  styles: []
})
export class AddOrUpdateNewsArticleListComponent implements OnInit {
  @Input() record: NzSafeAny;
  validateForm!: FormGroup;
  loading: boolean = false;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private ExtraNewsArticleListRepository : ExtraNewsArticleListRepository,
    private uploadRepository: UploadRepository,
    private UserService : UserService
  ) {}

  srcImg = this.UserService.getLoggedUser()['BaseUrlImg'];


  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      Code : [this.record ? this.record.Code : undefined,[]],
      TypeNews : [this.record ? this.record.TypeNews : undefined, []],
      Note : [this.record ? this.record.Note : undefined, []],
      FileImg : [this.record ? this.record.FileImg : undefined, []],
    });
  }
  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ExtraNewsArticleListRepository.update(data) : await this.ExtraNewsArticleListRepository.addNew(data);
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

  beforeUpload = (file: NzUploadFile): boolean => {
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadImage(formData);
    this.validateForm.get('FileImg')?.setValue(resp?.data.toString());
    this.cdr.detectChanges();
  }
}
