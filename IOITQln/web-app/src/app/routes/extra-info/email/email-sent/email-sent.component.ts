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
import { convertDate } from 'src/app/infrastructure/utils/common';
import { AddOrUpdateEmailSentComponent } from './add-or-update/add-or-update.component';


@Component({
  selector: 'app-email-sent',
  templateUrl: './email-sent.component.html',
  styles: [
  ]
})
export class EmailSentComponent implements OnInit {

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
    { title: 'Tiêu đề email', index: 'EmailHeader' },
    { title: 'Ngày gửi email', index: 'DateEmail' },
    { title: 'Nội dung email', index: 'ContentEmail' },
    {
      title: 'Chức năng',
      width: 80,
      className: 'text-center',
      buttons: [
        {
          icon: 'eye',
          iif: i => !i.edit,
          click: record => this.viewEmail(record)
        }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private modalSrv: NzModalService,
    private drawerService: NzDrawerService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Header: [undefined],
      From: [undefined],
      To: [undefined],

    });
    this.getData();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        this.viewEmail(e.dblClick?.item);
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
  search()
  {

  }

  async getData() {
    this.data=[
      {
       Id:1,
       DateEmail:convertDate((new Date().toString())),
       EmailHeader:'Thông báo công nợ tháng 9',
       ContentEmail:'Dear Mr.Hoang, chúng tôi thông báo ông Nguyễn Văn Hoàng sau khi đóng tiền thuê nhà tháng 9 là 3.000.000đ thì còn nợ 200.000đ của căn nhà A5, Quận 7',
      },
      {
       Id:2,
       DateEmail:convertDate((new Date().toString())),
       EmailHeader:'Thông báo công nợ tháng 9',
       ContentEmail:'Dear Mr.Chieu, Chúng tôi thông báo ông Nguyễn Việt Chiều sau khi đóng tiền thuê nhà tháng 9 là 3.500.000đ thì còn nợ 500.000đ của căn nhà 42A, Quận 9',
      },
      {
       Id:3,
       DateEmail:convertDate((new Date().toString())),
       EmailHeader:'Thông báo công nợ tháng 9',
       ContentEmail:'Dear Mr.Dung, Chúng tôi thông báo ông Phạm Việt Dũng sau khi đóng tiền thuê nhà tháng 9 là 3.300.000đ thì còn nợ 800.000đ của căn nhà 82A, Bình Tránh',
      },
    ]
    // this.paging.query = '1=1';
    // this.paging.order_by = 'CreatedAt Desc';

    // if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
    //   if (this.query.txtSearch.trim() != '')
    //     this.paging.query += ` and (Code.Contains("${this.query.txtSearch}")` + ` or Code.Contains("${this.query.txtSearch}"))`;
    // }

    // try {
    //   this.loading = true;
    //   const resp = await this.md167HouseTypeRepository.getByPage(this.paging);

    //   if (resp.meta?.error_code == 200) {
    //     this.data = resp.data;
    //     this.paging.item_count = resp.metadata;
    //   } else {
    //     this.modalSrv.error({
    //       nzTitle: 'Không lấy được dữ liệu.'
    //     });
    //   }
    // } catch (error) {
    //   throw error;
    // } finally {
    //   this.loading = false;
    // }
  }

  viewEmail(record: any): void {
    const drawerRef = this.drawerService.create<AddOrUpdateEmailSentComponent>({
      nzTitle: record ? `Sửa loại nhà: "${record.Name}"` : 'Thêm mới loại nhà',
      // record.khoa_chinh
      nzWidth: '80vw',
      nzContent: AddOrUpdateEmailSentComponent,
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
    // const resp = await this.md167HouseTypeRepository.delete(data);
    // if (resp.meta?.error_code == 200) {
    //   this.message.create('success', `Xóa loại nhà "${data.Name}" thành công!`);
    //   this.getData();
    // } else {
    //   this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    // }
  }

  onBack() {
    window.history.back();
  }
}

