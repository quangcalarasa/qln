import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HolidayRepository } from 'src/app/infrastructure/repositories/holiday.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import 'ag-grid-enterprise';
import { ICellRendererParams, SideBarDef, StatusPanelDef } from 'ag-grid-community';
import { ColDef, GridReadyEvent } from 'ag-grid-community';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-add-or-update-ag-grid',
    templateUrl: './ag-grid.component.html'
})

export class AgGridComponent implements OnInit {

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private http: HttpClient) { }

    ngOnInit(): void {
    }

    public statusBar: {
        statusPanels: StatusPanelDef[];
    } = {
            statusPanels: [
                {
                    statusPanel: 'agAggregationComponent',
                    statusPanelParams: {
                        aggFuncs: ['count', 'sum'],
                    },
                },
            ],
        };

    public columnDefs: ColDef[] = [
        {
            field: 'athlete', rowGroup: false, enableRowGroup: true
        },
        {
            field: 'age',
            filter: 'agNumberColumnFilter', rowGroup: false, enableRowGroup: true
        },
        {
            field: 'country'
        },
        { field: 'year', rowGroup: false, enableRowGroup: true },
        { field: 'date' },
        { field: 'sport' },
        { field: 'gold' },
        { field: 'silver' },
        { field: 'bronze' },
        { field: 'total' },
    ];
    public autoGroupColumnDef: ColDef = {
        minWidth: 200,
    };

    public defaultColDef: ColDef = {
        sortable: true,
        filter: true,
        resizable: true
    };

    public rowGroupPanelShow: 'always' | 'onlyWhenGrouping' | 'never' = 'always';

    public rowData$!: Observable<any[]>;

    onGridReady(params: GridReadyEvent) {
        this.rowData$ = this.http
            .get<any[]>('https://www.ag-grid.com/example-assets/olympic-winners.json');
        params.columnApi.autoSizeColumns(['age', 'country', 'year'])
    }
}
