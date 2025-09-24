import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';
import { AddOrUpdateTemplateComponent } from './add-or-update/add-or-update.component';
import { ExtraTemplateRepository } from 'src/app/infrastructure/repositories/extra-template.repository';


@Component({
  selector: 'app-email-template',
  templateUrl: './email-template.component.html',
  styles: [
  ]
})
export class EmailTemplateComponent implements OnInit {

  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  loading = false;

  TypeReportApply = TypeReportApply;

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Tên template', index: 'Name' },
    { title: 'Tiêu đề', index: 'Header' },
    { title: 'Nội dung', index: 'ContentShort'},
    {
      title: 'Chức năng',
      width: 80,
      className: 'text-center',
      buttons: [
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: record => this.addOrUpdate(record)
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá loại nhà này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private drawerService: NzDrawerService,
    private extraTemplateRepository: ExtraTemplateRepository,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        this.addOrUpdate(e.dblClick?.item);
        break;
      case 'checkbox':
        break;
      default:
        break;
    }
  }

  reset(): void {
    this.query = new QueryModel();
    this.paging.page = 1;
    this.getData();
  }

  searchData() {
    this.paging.page = 1;
    this.getData();
  }

  async getData() {
    this.paging.query = '1=1';
    this.paging.order_by = 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '')
        this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    }

    try {
      this.loading = true;
      const resp = await this.extraTemplateRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
        for(let i=0;i<this.data.length;i++)
        {
          this.data[i].ContentShort=this.truncateText(this.data[i].Content);
        }
        this.paging.item_count = resp.metadata;
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu.'
        });
      }
    } catch (error) {
      throw error;
    } finally {
      this.loading = false;
    }
  }
  truncateText(text: any){
    if (text.length > 9) {
      return text.substring(0, 9) + '...';
    }
    return text;
  }
  addOrUpdate(record?: any): void {
    const drawerRef = this.drawerService.create<AddOrUpdateTemplateComponent>({
      nzTitle: record ? `Sửa loại nhà: "${record.Name}"` : 'Thêm mới loại nhà',
      // record.khoa_chinh
      nzWidth: '80vw',
      nzContent: AddOrUpdateTemplateComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa loại nhà "${data.Name}" thành công!` : `Thêm mới loại nhà "${data.Name}" thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.extraTemplateRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa loại nhà "${data.Name}" thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
}

