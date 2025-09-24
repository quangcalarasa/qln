import { Component } from "@angular/core";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams } from "ag-grid-community";


@Component({
    selector: 'select-ag-grid',
    template: `<nz-select nzShowSearch nzAllowClear nzPlaceHolder="Vui lòng chọn" 
    [ngModel]="selectedValue"
    [nzBorderless]="true"
    (ngModelChange)="ngModelChange"
     [nzOptions]="nzOptions" >
  </nz-select>`
})
// eslint-disable-next-line @angular-eslint/component-class-suffix
export class SelectAgGrid implements ICellRendererAngularComp {
    private cellValue: string | undefined;
    nzOptions: any = [];
    selectedValue: any;
    // gets called once before the renderer is used
    agInit(params: ICellRendererParams): void {
        this.cellValue = this.getValueToDisplay(params);
        console.log(params);

        this.nzOptions = (params as any).values || [];
        // this.selectedValue = params.data['functionParent']['FunctionId'];
    }

    // gets called whenever the cell refreshes
    refresh(params: ICellRendererParams): boolean {
        this.cellValue = this.getValueToDisplay(params);
        return true;
    }

    getValueToDisplay(params: ICellRendererParams) {
        return params.valueFormatted ? params.valueFormatted : params.value;
    }

    getValue() {
        console.log(this.selectedValue);
        console.log(this.cellValue);

        return this.selectedValue;
    }
    ngModelChange(params: any) {
        console.log(params);
    }
}