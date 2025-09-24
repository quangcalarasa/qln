import { DebtComponent } from './../debt/debt.component';
import { RentTableComponent } from './../rent-table/rent-table.component';
import { Component, OnInit, ViewChild, Input, OnChanges, SimpleChanges } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TypeReportApply, FileStatus } from 'src/app/shared/utils/consts';
import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { TypeBlockRepository } from 'src/app/infrastructure/repositories/type-block.repository';
import { AddendumComponent } from '../addendum/addendum.component';

@Component({
  selector: 'app-list-addendum',
  templateUrl: './list-addendum.component.html',
  styles: []
})
export class ListAddendumComponent implements OnInit, OnChanges {
  @ViewChild('tableRef', { static: false }) tableRef!: STComponent;
  @Input() code: string;
  @Input() id : number;
  @Input() change: boolean = false;

  validateForm!: FormGroup;
  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  data: any;
  loading = false;

  typeReportApply = TypeReportApply;
  fileStatus = FileStatus;

  typehouse_data: any[] = [];

  columns: STColumn[] = [
    { title: 'Stt', type: 'no', width: 40 },
    { title: 'Mã hồ sơ', index: 'CodeHS' },
    { title: 'Mã hợp đồng', index: 'Code' },
    { title: 'Trạng thái hồ sơ', index: 'FileStatus', type: 'enum', enum: FileStatus },
    { title: 'Loại nhà', index: 'TypeBlockId', render: 'typeblock-column' },
    { title: 'Ngày tạo', index: 'DateHD', type: 'date', dateFormat: 'dd/MM/yyyy' },
    { title: 'Ngày cập nhập', index: 'UpdatedAt', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { renderTitle: 'useAreaValueHeader', render: 'uavClmn', className: 'text-center' },
    {
      title: 'Chức năng',
      width: 100,
      className: 'text-center',
      buttons: [
        {
          icon: 'table',
          iif: i => !i.edit,
          click: record => this.getWorkSheet(record),
          tooltip: 'Bảng chiết tính'
        },
        {
          icon: 'edit',
          iif: i => !i.edit,
          click: data => this.Addendum(data)
        },
        {
          icon: 'delete',
          type: 'del',
          pop: {
            title: 'Bạn có chắc chắn muốn xoá phụ lục này?',
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
    private rentFileRepository: RentFileRepository,
    private drawerService: NzDrawerService,
    private message: NzMessageService,
    private TypeBlockRepository: TypeBlockRepository
  ) { }

  ngOnInit(): void {
    this.getTypeHouse();
  }
  ngOnChanges(changes: SimpleChanges): void {
    this.getData();
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        break;
      case 'checkbox':
        break;
      default:
        break;
    }
  }

  async getData() {
    if (this.id == null) this.id = 0;
    this.paging.page_size = 0;
    this.paging.query = `Type=${2} AND ParentId.ToString().Contains("${this.id}")`;
    this.paging.order_by = 'CreatedAt Desc';
    try {
      this.loading = true;
      const resp = await this.rentFileRepository.getByPage(this.paging);
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

  getWorkSheet(record?: any): void {
    const drawerRef = this.drawerService.create<RentTableComponent>({
      nzTitle: 'Bảng chiết tính',
      nzWidth: '100vw',
      nzContent: RentTableComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  debt(record?: any): void {
    let add;
    record ? (add = true) : (add = false);
    localStorage.setItem('add', add.toString());
    const drawerRef = this.drawerService.create<DebtComponent>({
      nzTitle: 'Công nợ',
      nzWidth: '85vw',
      nzContent: DebtComponent,
      nzPlacement: 'left',
      nzContentParams: {
        record
      }
    });
  }

  async delete(data: any) {
    const resp = await this.rentFileRepository.delete(data);
    if (resp.meta?.error_code == 200) {
      this.message.create('success', `Xóa phụ lục hồ sơ giá thuê ${data.Code} thành công!`);
      this.getData();
    } else {
      this.message.create('error', resp.meta?.error_message ? resp.meta?.error_message : '');
    }
  }

  Addendum(data?: any) {
    this.modalSrv.create({
      nzTitle: `Phụ lục cho hợp đồng "${this.code}"`,
      nzContent: AddendumComponent,
      nzWidth: '75vw',
      nzComponentParams: {
        record : data,
        parent : data,
        typehouse_data: this.typehouse_data
      },
      nzOnOk: (res: any) => {
        this.message.create('success', `Thêm phụ lục cho hợp đồng ${data.Code} thành công!`);
        this.getData();
      }
    });
  }

  async getTypeHouse() {
    let paging: GetByPageModel = new GetByPageModel();

    const resp = await this.TypeBlockRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.typehouse_data = resp.data;
    } else {
      this.modalSrv.error({
        nzTitle: 'Không lấy được dữ liệu loại căn.'
      });
    }
  }

  genTypeBlock(typeBlockId: number) {
    let typeblock = this.typehouse_data.find(x => x.Id == typeBlockId);

    return typeblock ? typeblock.Name : '';
  }
}
