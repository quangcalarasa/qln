import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ExtraPostingRepository } from 'src/app/infrastructure/repositories/ExtraPostingRepository';

@Component({
  selector: 'extra-info/News/Posting-configuration',
  templateUrl: './PostingConfiguration.component.html',
  styles: []
})
export class PostingConfiguration implements OnInit {

  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data=  [
    {}
  ];
  loading = false;

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private ExtraPostingRepository : ExtraPostingRepository
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      CapacityImg : [],
      CapacityVideo : [],
      CapacityFile : [],
    });
  }

 async saveData(){
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.ExtraPostingRepository.update(data) : await this.ExtraPostingRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.validateForm.reset('CapacityImg');
      this.validateForm.reset('CapacityVideo');
      this.validateForm.reset('CapacityFile');
      this.loading = false;
      this.message.success(`Lưu cấu hình đăng tin thành công!`);
      
    } else {
      this.loading = false;
    }
  }
}
