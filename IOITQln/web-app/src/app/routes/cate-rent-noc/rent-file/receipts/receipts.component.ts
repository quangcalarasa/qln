import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { SettingsService } from '@delon/theme';
import { PromissoryRepository } from '../../../../infrastructure/repositories/Promissory.repository';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';

@Component({
  selector: 'app-receipts',
  templateUrl: './receipts.component.html',
  styles: []
})
export class ReceiptsComponent implements OnInit {
  @Input() code: string;
  @Input() SurplusBalance: NzSafeAny;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  validateForm!: FormGroup;
  loading: boolean = false;
  newDate = convertDate(new Date().toString());

  valueMax: any;
  data: any[] = [];

  columns: STColumn[] = [
    { title: 'STT', type: 'no', width: 40 },
    { title: 'Ngày rút tiền', index: 'Date', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Người thực hiện', index: 'Executor' },
    { renderTitle: 'PriceHeader', render: 'Price' }
  ];

  constructor(
    private fb: FormBuilder,
    private message: NzMessageService,
    private settings: SettingsService,
    private promissoryRepository: PromissoryRepository,
    private modal: NzModalRef
  ) {}

  get user(): any {
    return this.settings.user;
  }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Code: [this.code],
      Date: [new Date().toISOString().slice(0, 10)],
      Executor: [this.user.FullName],
      Price: [undefined],
      Action: [2] //1 là thu - 2 là chi
    });
    this.valueMax = this.SurplusBalance;
    this.getData();
  }

  async submitForm() {
    let data = { ...this.validateForm.value };
    try {
      this.loading = true;
      const resp = await this.promissoryRepository.addNew(data);

      if (resp.meta?.error_code == 200) {
        this.modal.triggerOk();
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }

  close(): void {
    this.modal.close();
  }

  changePrice(env: any) {
    if (env > this.valueMax) {
      this.message.error('Số tiền rút ra không được lớn hơn số dư treo!!!!');
      this.validateForm.get('Price')?.setValue(this.valueMax);
    } else {
      this.validateForm.get('Price')?.setValue(env);
    }
  }

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `Action=${2} AND  Code.Contains("${this.code}")`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.promissoryRepository.getByPage(this.paging);
      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }
}
