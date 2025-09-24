import { Component } from '@angular/core';
import { ILoadingOverlayAngularComp } from 'ag-grid-angular';
import { ILoadingOverlayParams } from 'ag-grid-community';

@Component({
  selector: 'app-shared-aggrid-loading-overlay',
  template: `
    <div class="ag-overlay-loading-center">
      <div
        style="width: 120px; height: 100px; background: url(./assets/ag-grid-loading-spinner.svg) center / contain no-repeat; margin: 0 auto;"
        aria-label="loading"
      ></div>
      <div>Đang lập báo cáo</div>
    </div>
  `,
})
export class SharedAgGridLoadingOverlayComponent implements ILoadingOverlayAngularComp {

  agInit(): void {}
}