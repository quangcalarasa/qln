import { Component } from "@angular/core";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams } from "ag-grid-community";

@Component({
    selector: 'checkbox-editor',
    template: '<label nz-checkbox [(ngModel)]="checked"></label>',
})
export class CheckboxEditor implements ICellRendererAngularComp {
    params: any;
    checked!: Boolean;
    agInit(params: any): void {
        this.params = params;
        this.checked = !params.value;
    }

    refresh(params: ICellRendererParams): boolean {
        return true;
    }

    getValue() {
        return this.checked
    }
}