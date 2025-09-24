import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder,  FormGroup, Validators } from '@angular/forms';
import { QuickmathRepository } from 'src/app/infrastructure/repositories/qiuckmath.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
@Component({
  selector: 'AddQuickMath',
  templateUrl: './AddQuickMath.component.html',
  styles: []
})
export class AddQuickMathComponent implements OnInit {
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
    this.validateForm = this.fb.group({
      Value: [ undefined, [Validators.required]],
      DoApply: [ undefined, [Validators.required]],
      Type: [undefined,[Validators.required]],
    });
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.getRawValue() };
    const resp =  await this.QuickmathRepository.addNew(data);
    if (resp.meta?.error_code == 200) {
      this.isVisible = false;
      this.isOkLoading = false;
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }

  async getView(){
    let data = { ...this.validateForm.getRawValue() };
    const resp =  await this.QuickmathRepository.getView(data);
    if(resp.meta?.error_code == 200){
      this.ViewData = resp.data;
      console.log(this.ViewData);
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  showModal(): void {
    this.isVisible = true;
  }

  handleOk(): void {
    this.isOkLoading = true;
    this.submitForm()
  }

  handleCancel(): void {
    this.isVisible = false;
  }

  genValue(value1 : any, value2? : any){
    if(value2 != null){
      this.oldValue = value2;
    }
    let a  : any;
    return a = ((value1 * this.validateForm.value.Value) / this.oldValue);
  }
}
