import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { AreaRepository } from 'src/app/infrastructure/repositories/area.repository';
import { NzModalRef } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-choose-area-block',
  template: `
    <div nz-row [nzGutter]="24" style="max-height: 400px;overflow-y: scroll;">
      <div nz-col [nzSpan]="24">
      <nz-tree
        [nzData]="nodes"
        nzCheckable
        nzMultiple
      ></nz-tree>
      </div>
    </div>
    <div *nzModalFooter>
      <button nz-button nzType="default" (click)="close()">Hủy</button>
      <button nz-button nzType="primary" (click)="submit()">Chọn</button>
    </div>
  `
})
export class ChooseAreaBlockComponent implements OnInit {

  nodes = [];

  constructor(
    private areaRepository: AreaRepository,
    private modal: NzModalRef
  ) { }

  ngOnInit(): void {
    this.getDataNzTree();
  }

  async submit() {
    this.modal.triggerOk();
  }

  close(): void {
    this.modal.close();
  }

  async getDataNzTree() {
    const resp = await this.areaRepository.getDataNzTree();

    if (resp.meta?.error_code == 200) {
      this.nodes = resp.data;
    }
  }
}
