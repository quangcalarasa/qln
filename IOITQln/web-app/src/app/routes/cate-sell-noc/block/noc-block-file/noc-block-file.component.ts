import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { STChange, STColumn, STComponent, STData } from '@delon/abc/st';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NocBlockFileRepository } from 'src/app/infrastructure/repositories/NocBlockFile.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { NzModalService } from 'ng-zorro-antd/modal';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';


@Component({
  selector: 'app-noc-block-file',
  templateUrl: './noc-block-file.component.html',
  styles: [
  ]
})
export class NocBlockFileComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

  validateForm!: FormGroup;
  loading: boolean = false;
  @Input() record: NzSafeAny;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();
  data: any[] = [];
  check = false;
  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Tên thư mục file', index: 'NameFile'},
    { title: 'Ngày cập nhật', index: 'UpdatedAt', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { title: 'Người cập nhật', index: 'UpdatedBy'},
    { title: 'Ghi chú', index: 'Note'},
    { title: 'Tải file', render: 'attachedfiles-column'},
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xóa file này?',
            okType: 'danger',
            icon: 'star'
          },
          click: record => this.delete(record)
        },
      ]
    }
  ];
  constructor(
    private nocBlockFileRepository: NocBlockFileRepository,
    private blockRepository: BlockRepository,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
    private message: NzMessageService,
    private modalSrv: NzModalService,
    private uploadRepository: UploadRepository,
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      NameFile: [this.record ? this.record.NameFile : undefined, [Validators.required]],
      NocBlockId: [this.record ? this.record.Id : undefined],
      AttachedFiles: [this.record ? this.record.AttachedFiles : undefined, [Validators.required]],
      Note: [this.record ? this.record.Note : undefined],
    });
    this.getData();
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };
    const resp = await this.nocBlockFileRepository.addNew(data);
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

  async getData() {
    this.paging.page_size = 0;
    this.paging.query = `NocBlockId=${this.record.Id}`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.nocBlockFileRepository.getByPage(this.paging);

      if (resp.meta?.error_code == 200) {
        this.data = resp.data;
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

  async delete(data: any) {
    const resp = await this.nocBlockFileRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa file đính kèm thành công!`);
      this.close();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      default:
        break;
    }
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    console.log(file);
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    const resp = await this.uploadRepository.uploadFile(formData);
    this.validateForm.get('AttachedFiles')?.setValue(resp?.data.toString());

  }

  downloadFile(fileName: string) {
    this.uploadRepository.downloadFile(fileName);
  }

}
