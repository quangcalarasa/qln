import { Component, Input, OnInit, ChangeDetectorRef, ViewChild, Type } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { Md167ReceiptComponent } from '../receipt/receipt.component';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzMessageService } from 'ng-zorro-antd/message';
import { EventMd167ReceiptService } from 'src/app/core/services/event-md167Receipt.service';

@Component({
  selector: 'app-md167-contract-debt-table',
  templateUrl: './debt-table.component.html',
  styles: []
})
export class Md167ContractDebtTableComponent implements OnInit {
  @Input() record: NzSafeAny;
  data: any[] = [];

  constructor(private md167ContractRepository: Md167ContractRepository, private drawerRef: NzDrawerRef<string>, private modalSrv: NzModalService,  private message: NzMessageService, private eventMd167ReceiptService: EventMd167ReceiptService) {
    this.eventMd167ReceiptService.getMessage().subscribe(message => { this.getData(); });
  }

  ngOnInit(): void {
    this.getData();
  }

  async getData() {
    const resp = await this.md167ContractRepository.GetDataDebt(this.record.Id);
    if (resp.meta?.error_code == 200) {
      this.data = resp.data;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  payment() {
    this.modalSrv.create({
      nzTitle: `Thông tin thanh toán của hợp đồng "${this.record.Code}"`,
      nzContent: Md167ReceiptComponent,
      nzWidth: '55vw',
      nzComponentParams: {
        record: this.record,
      },
      nzOnOk: (res: any) => {
        this.getData();
      }
    });
  }

  exportReport() {
    this.md167ContractRepository.exportReport(this.record.Id);
  }

  async refundPaidDeposit() {
    const resp = await this.md167ContractRepository.refundPaidDeposit(this.record);
    if (resp.meta?.error_code == 200) { 
      this.message.success("Đổi trạng thái hoàn tiền thành công!");
    }
    else {
      this.record.RefundPaidDeposit = !this.record.RefundPaidDeposit;
    }
  }
}
