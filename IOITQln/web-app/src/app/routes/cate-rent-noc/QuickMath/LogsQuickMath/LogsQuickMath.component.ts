import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder,  FormGroup, Validators } from '@angular/forms';
import { QuickmathRepository } from 'src/app/infrastructure/repositories/qiuckmath.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
  selector: 'AddQuickMath',
  templateUrl: './LogsQuickMath.component.html',
  styles: []
})
export class LogsQuickMathComponent implements OnInit {
  @Input() record: NzSafeAny;
  validateForm!: FormGroup;
  loading: boolean = false;
  isVisible = false;
  isOkLoading = false;
  oldValue : any;

  public ViewData : any;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private QuickmathRepository : QuickmathRepository,
    private message: NzMessageService
  ) {}

  ngOnInit(): void {
    this.getView();
  }

  async getView(){
    let data = {
      QuickMathHistoryId  : this.record.Id,
    };
    const resp =  await this.QuickmathRepository.getLogs(data);
    if(resp.meta?.error_code == 200){
      this.ViewData = resp.data;
      console.log(this.ViewData);
    }
  }

  close(): void {
    this.drawerRef.close();
  }
}
