import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzMessageService } from 'ng-zorro-antd/message';

import { ExtraNewsArticleRepository } from 'src/app/infrastructure/repositories/ExtraNewsArticle.repository';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { UserService } from 'src/app/core/services/user.service';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';


@Component({
  selector: 'extra-info/News/News-article/add-or-update',
  templateUrl: './add-or-update-NewsArticle.component.html',
  styles: []
})
export class AddOrUpdateNewsArticleComponent implements OnInit {
  @Input() record: NzSafeAny;
  validateForm!: FormGroup;
  loading: boolean = false;

  @Input() dataTypeNews : NzSafeAny;
  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private message: NzMessageService,
    private ExtraNewsArticleRepository: ExtraNewsArticleRepository,
    private uploadRepository: UploadRepository,
    private UserService : UserService
  ) {}

  srcImg = this.UserService.getLoggedUser()['BaseUrlImg'];


  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      ArticleTitle : [this.record ? this.record.ArticleTitle : undefined,[]],
      ExtraNewsArticleListId : [this.record ? this.record.ExtraNewsArticleListId : undefined, []],
      ShortNote : [this.record ? this.record.ShortNote : undefined, [],],
      Files : [this.record ? this.record.Files : undefined, [],],
      Content : [this.record ? this.record.Content : undefined, [],],
      DateUpdate : [this.record ? convertDate(this.record.DateUpdate) : undefined, [],],
    });
  
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ExtraNewsArticleRepository.update(data) : await this.ExtraNewsArticleRepository.addNew(data);
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
    this.validateForm.get('Files')?.setValue(resp?.data.toString());
    this.cdr.detectChanges();
  }
}
