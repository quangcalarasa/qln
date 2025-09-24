import { Component, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { FileStatus } from 'src/app/shared/utils/consts';
import { TypeAttributeRepository } from 'src/app/infrastructure/repositories/type-attribute.repository';
import { TypeAttributeCode } from 'src/app/shared/utils/enums';
import { TypeBlockEntityEnum, CodeStatusEnum } from 'src/app/shared/utils/enums';
import { FilesRepository } from './../../../infrastructure/repositories/Files.repositories';
import { AddOrUpdateFilesComponent } from './add-or-update-files/add-or-update-files.component';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';

@Component({
  selector: 'app-files',
  templateUrl: './files.component.html',
  styles: []
})
export class FilesComponent implements OnInit {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any[] = [];
  typehouse_data: any[] = [];

  loading = false;

  fileStatus = FileStatus;

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã hồ sơ', index: 'CodeFile' },
    { title: 'Loại nhà', index: 'TypeBlockId', render: 'typeblock-column' },
    { title: 'Trạng thái', index: 'TypeFile', type: 'enum', enum: FileStatus },
    { title: 'Ngày tạo', index: 'CreatedAt', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { title: 'Ngày cập nhập', index: 'UpdatedAt', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { renderTitle: 'useAreaValueHeader', render: 'uavClmn', className: 'text-center' },
    { title: 'Ghi Chú', index: 'Note' },
    {
      title: 'Chức năng',
      width: 100,
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
            title: 'Bạn có chắc chắn muốn xoá căn nhà này?',
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
    private filesRepository: FilesRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private typeBlockRepository: TypeBlockRepository
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({});
    this.getData();
    this.getTypeBlockData();
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
    this.paging.order_by = this.paging.order_by ? this.paging.order_by : 'CreatedAt Desc';

    if (this.query.txtSearch != undefined && this.query.txtSearch != '') {
      if (this.query.txtSearch.trim() != '') this.paging.query += ` and CodeFile.Contains("${this.query.txtSearch}")`;
    }
    if (this.query.type != undefined) {
      this.paging.query += ` and TypeFile=${this.query.type}`;
    }
    try {
      this.loading = true;
      const resp = await this.filesRepository.getByPage(this.paging);

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

  addOrUpdate(record?: any): void {
    const drawerRef = this.drawerService.create<AddOrUpdateFilesComponent>({
      nzTitle: record ? `Sửa hồ sơ giá thuê: ${record.CodeFile}` : 'Thêm mới hồ sơ giá thuê',
      // record.khoa_chinh
      nzWidth: '75vw',
      nzContent: AddOrUpdateFilesComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record,
        typehouse_data: this.typehouse_data
      }
    });

    drawerRef.afterClose.subscribe((data: any) => {
      if (data) {
        let msg = data.Id ? `Sửa hồ sơ giá thuê ${data.CodeFile} thành công!` : `Thêm mới hồ sơ giá thuê ${data.CodeFile} thành công!`;
        this.message.success(msg);
        this.getData();
      }
    });
  }

  async delete(data: any) {
    const resp = await this.filesRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa hồ sơ giá thuê ${data.CodeFile} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  onBack() {
    window.history.back();
  }
  async getTypeBlockData() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    const resp = await this.typeBlockRepository.getByPage(paging);

    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    }
  }
  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find(x => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }
  paingSize(event: any) {
    this.paging.page_size = event;
    this.getData();
  }
}
