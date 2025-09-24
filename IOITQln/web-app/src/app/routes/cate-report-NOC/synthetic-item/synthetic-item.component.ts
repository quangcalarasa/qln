import { Component, Input, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { _HttpClient } from '@delon/theme';
import { NzModalRef } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-report-noc-synthetic-item',
  templateUrl: './synthetic-item.component.html',
})
export class SyntheticItemComponent implements OnInit {
  @Input() data: any;

  constructor(private sanitizer: DomSanitizer, private modal: NzModalRef) {
  }

  ngOnInit(): void {}

  close(): void {
    this.modal.close();
}
}
