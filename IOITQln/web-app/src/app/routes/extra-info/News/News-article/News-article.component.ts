import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';

import { AddOrUpdateNewsArticleComponent } from './add-or-update/add-or-update-NewsArticle.component';

import { ExtraNewsArticleRepository } from 'src/app/infrastructure/repositories/ExtraNewsArticle.repository';
import { ExtraNewsArticleListRepository } from 'src/app/infrastructure/repositories/ExtraNewsArticleList.repositories';


@Component({
  selector: 'extra-info/News/News-article',
  templateUrl: './News-article.component.html',
  styles: []
})
export class NewsArticleComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any =[];
  loading = false;
  dataTypeNews = [];

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Tiêu đề bài viết', index: 'ArticleTitle' },
    { title: 'Loại danh mục tin tức', index: 'TypeNewsName' },
    { title: 'Mô tả ngắn', index: 'ShortNote'},
    { title: 'Nội dung', index: 'Content'},
    { title: 'Ngày cập nhập', index: 'DateUpdate', type: 'date', dateFormat: 'dd/MM/yyyy' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
            icon: 'edit',
            iif: (i : any)=> !i.edit,
            click: (record : any) => this.addOrUpdate(record),
            tooltip : "Sửa"
          },
          {
            icon: 'delete',
            type: 'del',
            pop: {
              title: 'Bạn có chắc chắn muốn xoá?',
              okType: 'danger',
              icon: 'star'
            },
            click: (record : any) => this.delete(record),
            tooltip : "Xóa"
          }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private ExtraNewsArticleRepository : ExtraNewsArticleRepository,
    private ExtraNewsArticleListRepository : ExtraNewsArticleListRepository,

  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getDataNewArticleList();
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
    this.paging.page_size = 10;
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
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and ArticleTitle.Contains("${this.query.txtSearch}")`;
    }

    try {
      this.loading = true;
      const resp = await this.ExtraNewsArticleRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
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

  addOrUpdate(record? : any): void {
    const drawerRef = this.drawerService.create<AddOrUpdateNewsArticleComponent>({
      nzTitle: record ? `Sửa ` : 'Thêm mới ',
      // record.khoa_chinh
      nzWidth: '55vw',
      nzContent: AddOrUpdateNewsArticleComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        dataTypeNews : this.dataTypeNews
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa thành công!` : `Thêm mới thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }


  async delete(data: any) {
    const resp = await this.ExtraNewsArticleRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa  thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }

  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }

  async getDataNewArticleList(){
    let paging: GetByPageModel = new GetByPageModel();
    const resp = await this.ExtraNewsArticleListRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.dataTypeNews = resp.data;
    }
}
}
