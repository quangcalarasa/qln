import { Component, ElementRef, ViewChild } from "@angular/core";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams } from "ag-grid-community";
import { BehaviorSubject, debounceTime, distinctUntilChanged, } from "rxjs";

@Component({
    selector: 'autocomplete-editor',
    templateUrl: './autocomplete-editor.component.html',
})
export class AutocompleAgGrid implements ICellRendererAngularComp {
    @ViewChild('myInput') myInput!: ElementRef;

    cellValue: string | undefined;
    cellId: any;
    params: any;
    searchChange$ = new BehaviorSubject('');
    optionList: any[] = [];
    isLoading = false;
    keyField = "Id";
    labelField = 'Name';
    onSelect: any;

    ngAfterViewInit() {
        window.setTimeout(() => {
            this.myInput?.nativeElement.focus();
        });
    }

    agInit(params: any): void {
        this.keyField = params.keyField;
        this.labelField = params.labelField;
        this.onSelect = params.onSelect;
        this.params = params;

        if (!params.charPress) {
            this.cellValue = params.valueFormatted ? params.valueFormatted : params.value;
        } else {
            this.cellValue = params.charPress;
            this.searchChange$.next(params.charPress);
        }

        this.searchChange$.pipe(
            debounceTime(400),
            distinctUntilChanged()
        ).subscribe(async term => {
            this.optionList = await (params as any).onSearch(term);
            this.isLoading = false;
        });
    }

    refresh(params: ICellRendererParams): boolean {
        // this.cellValue = this.getValueToDisplay(params);
        return true;
    }


    getValue() {
        let obj = this.optionList.find(item => item[this.keyField] == this.cellValue);
        if (!obj) return this.params.value;

        if (this.onSelect(this.params.rowIndex, obj))
            return this.params.value;
    }

    processDataInput(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.isLoading = true;
        this.searchChange$.next(value);
    }
}