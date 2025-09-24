import { Component, Input, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { STChange, STColumn, STComponent, STData, STSingleSort } from '@delon/abc/st';
import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzModalService } from 'ng-zorro-antd/modal';

import { BlockRepository } from 'src/app/infrastructure/repositories/block.repository';
import { FileStatus, TypeReportApply, LevelBlock } from 'src/app/shared/utils/consts';
import { FilesRepository } from 'src/app/infrastructure/repositories/Files.repositories';

@Component({
  selector: 'app-add-or-update-files',
  templateUrl: './add-or-update-files.component.html',
  styles: []
})
export class AddOrUpdateFilesComponent implements OnInit {
  @ViewChild('tableItemRef') private tableItemRef!: STComponent;

  public add = true;
  validateForm!: FormGroup;
  loading: boolean = false;

  block_data: any[] = [];
  block_Filter_data: any[] = [];
  data_filter_block: any[] = [];

  FileStatus = FileStatus;
  TypeReportApply = TypeReportApply;
  level_data = LevelBlock;

  @Input() record: NzSafeAny;
  @Input() typehouse_data: NzSafeAny;

  constructor(
    private drawerRef: NzDrawerRef<string>,
    private fb: FormBuilder,
    private filesRepository: FilesRepository,
    private modalSrv: NzModalService,
    private blockRepository: BlockRepository
  ) {}

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      Id: [this.record ? this.record.Id : undefined],
      BlockId: [this.record ? this.record.BlockId : undefined, [Validators.required]],
      TypeBlockId: [this.record ? this.record.TypeBlockId : undefined, [Validators.required]],
      CodeFile: [this.record ? this.record.CodeFile : undefined, [Validators.required]],
      TypeReportApply: [this.record ? this.record.TypeReportApply.toString() : undefined, [Validators.required]],
      TypeFile: [this.record ? this.record.TypeFile.toString() : undefined],
      fullAddress: [this.record ? this.record.fullAddress : undefined, [Validators.required]],
      Date: [this.record ? convertDate(this.record.Date) : undefined],
      UseAreaValue: [this.record ? this.record.UseAreaValue : undefined],
      Note: [this.record ? this.record.Note : undefined]
    });
    this.getDataBlock();
    if (this.validateForm.value.BlockId) {
      this.GetBlockDataFilter(this.validateForm.value.BlockId);
    }
  }

  async submitForm() {
    this.loading = true;
    let data = { ...this.validateForm.value };

    const resp = data.Id ? await this.filesRepository.update(data) : await this.filesRepository.addNew(data);
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

  ///Căn hộ
  async getDataBlock() {
    let TypeReportApply = this.validateForm.value.TypeReportApply;
    let TypeBlockId = this.validateForm.value.TypeBlockId;
    if (TypeReportApply && TypeBlockId) {
      let paging: GetByPageModel = new GetByPageModel();
      paging.page_size = 0;
      paging.query = `TypeReportApply=${TypeReportApply} AND TypeBlockId=${TypeBlockId}`;
      const resp = await this.blockRepository.getByPage(paging);
      if (resp.meta?.error_code == 200) {
        this.block_data = [...resp.data];
      } else {
        this.modalSrv.error({
          nzTitle: 'Không lấy được dữ liệu căn nhà!!!'
        });
      }
    }
  }

  SetDataBlock() {
    if (this.data_filter_block) {
      this.validateForm.get('UseAreaValue')?.setValue(this.data_filter_block[0].UseAreaValue);
      this.validateForm.get('fullAddress')?.setValue(this.data_filter_block[0].FullAddress);
    }
  }

  async GetBlockDataFilter(event: any) {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.query = `Id=${event}`;
    const resp = await this.blockRepository.getByPage(paging);
    if (resp.meta?.error_code == 200) {
      this.block_Filter_data = [...resp.data[0].levelBlockMaps];
      this.data_filter_block = [...resp.data];
    }
    this.SetDataBlock();
  }

  compareFn = (o1: any, o2: any) => {
    return o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2;
  };
}
