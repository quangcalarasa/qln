import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportNocRepository } from 'src/app/infrastructure/repositories/report-noc.repository';
import { ColDef, GridReadyEvent, ColGroupDef } from 'ag-grid-community';
import { ICellRendererParams, SideBarDef, StatusPanelDef } from 'ag-grid-community';
import 'ag-grid-enterprise';
import { GridApi, ColumnApi } from 'ag-grid-community';
import { DatePipe } from '@angular/common';
import { TypeSex } from 'src/app/shared/utils/consts';
import { SharedAgGridLoadingOverlayComponent } from 'src/app/shared/components/ag-grid/loading-overlay';

@Component({
  selector: 'app-report-noc-customer',
  templateUrl: './customer.component.html',
  styles: []
})
export class CustomerReportComponent implements OnInit {
  validateForm!: FormGroup;

  loading = false;

  gridApi: GridApi;
  typeSexData = TypeSex;

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

  public columnDefs: (ColDef | ColGroupDef)[] = [
    { headerName: 'Stt', valueGetter: (arges) => { return (arges.node?.rowIndex ?? 0) + 1 }, onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Khách hàng', field: 'FullName', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Ngày sinh', field: 'Dob', valueFormatter: (data: any) => {
      return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
    }, onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Giới tính', field: 'Sex', valueFormatter: (data: any) => {
      return data.value == 1 ? "Nam" : ( data.value == 2 ? "Nữ" : ( data.value == 3 ? "Khác" :""));
    }, onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Số điện thoại', field: 'Phone', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Email', field: 'Email', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Địa chỉ', field: 'Address', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'CCCD/CMND', field: 'Code', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Ngày cấp', field: 'Doc', valueFormatter: (data: any) => {
      return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
    }, onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Nơi cấp', field: 'PlaceCode', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Hợp đồng', field: 'ContractCode', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Ngày ký hợp đồng', field: 'ContractDate', valueFormatter: (data: any) => {
      return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
    }, onCellClicked: (evt) => { this.viewRow(evt) } }
  ];
  public autoGroupColumnDef: ColDef = {
    minWidth: 200,
  };

  public defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  public sideBar: SideBarDef | string | string[] | boolean | null = {
    toolPanels: [
      {
        id: 'columns',
        labelDefault: 'Cột thông tin',
        labelKey: 'columns',
        iconKey: 'columns',
        toolPanel: 'agColumnsToolPanel',
        toolPanelParams: {
          suppressRowGroups: true,
          suppressValues: true,
          suppressPivots: true,
          suppressPivotMode: true,
          suppressColumnFilter: true,
          suppressColumnSelectAll: true,
          suppressColumnExpandAll: true,
        },
      },
    ]
  };

  public rowGroupPanelShow: 'always' | 'onlyWhenGrouping' | 'never' = 'always';

  public rowData$: any[] = [];

  public loadingOverlayComponent: any = SharedAgGridLoadingOverlayComponent;

  constructor(
    private reportNocRepository: ReportNocRepository,
    private fb: FormBuilder,
    private datePipe: DatePipe
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      StartDate: [undefined],
      EndDate: [undefined],
      CustomerName: [undefined],
      Phone: [undefined],
      IdentityCode: [undefined],
      Sex: [undefined]
    });
  }

  async getDataReport() {
    this.loading = true;
    // this.rowData$ = [];
    this.gridApi.setRowData([]);
    this.gridApi.showLoadingOverlay();
    const resp = await this.reportNocRepository.GetCustomerNocReport(this.validateForm.value);
    if (resp.meta?.error_code == 200) {
      this.rowData$ = resp.data;
      this.loading = false;
    }
    else {
      this.loading = false;
    }

    this.gridApi.hideOverlay();
  }

  async dowloadReport() {
    this.gridApi.exportDataAsExcel();
  }

  onGridReady(params: GridReadyEvent) {
    this.gridApi = params.api;
    // this.gridApi.sizeColumnsToFit();
  }

  viewRow(event: any) {
    
  }
}
