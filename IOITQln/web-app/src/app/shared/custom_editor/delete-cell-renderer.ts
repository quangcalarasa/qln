import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

@Component({
  selector: 'total-value-component',
  template: `
    <i (click)="onClick($event)" class='text-error-dark' nz-icon nzType="delete" nzTheme="fill"></i>`
})

export class DeleteCellRenderer implements ICellRendererAngularComp {
  params?: any;
  agInit(params: ICellRendererParams): void {
    this.params = params;
  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  onClick($event: any) {
    if (this.params?.onDeleteRow instanceof Function) {
      this.params?.onDeleteRow(this.params.node.data);
    }
  }
}
