import { Component, OnInit } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { FormBuilder, FormGroup } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { ReportNocRepository } from 'src/app/infrastructure/repositories/report-noc.repository';
import { ColDef, GridReadyEvent, ColGroupDef } from 'ag-grid-community';
import { ICellRendererParams, SideBarDef, StatusPanelDef } from 'ag-grid-community';
import 'ag-grid-enterprise';
import { HttpClient } from '@angular/common/http';
import { GridApi, ColumnApi } from 'ag-grid-community';
import { NzModalService } from 'ng-zorro-antd/modal';
import { DatePipe } from '@angular/common';
import { UsageStatus } from 'src/app/shared/utils/consts';
import { SharedAgGridLoadingOverlayComponent } from 'src/app/shared/components/ag-grid/loading-overlay';

@Component({
  selector: 'app-report-noc-due-contract',
  templateUrl: './due-contract.component.html',
  styles: []
})
export class DueContractReportComponent implements OnInit {
  validateForm!: FormGroup;
  districts: any;
  wards: any;
  lanes: any;

  query: QueryModel = new QueryModel();
  loading = false;

  gridApi: GridApi;

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
      { headerName: 'Mã định danh', field: 'Code', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Số nhà', field: 'Address', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Đường', field: 'Lane', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Phường/xã', field: 'Ward', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Quận/huyện', field: 'District', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Hợp đồng', field: 'CodeHS', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Chủ hộ', field: 'CustomerName', onCellClicked: (evt) => { this.viewRow(evt) } },
      { headerName: 'Hạn thanh toán', field: 'PaymentDeadline', valueFormatter: (data: any) => {
        return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
      }, onCellClicked: (evt) => { this.viewRow(evt) } },
      {
        headerName: 'Số tiền phải nộp', field: 'Total', valueFormatter: (data: any) => {
          return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
        }, onCellClicked: (evt) => { this.viewRow(evt) }
      },
      // { headerName: 'Địa chỉ', field: '', onCellClicked: (evt) => { this.viewRow(evt) } }
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
    private wardRepository: WardRepository,
    private laneRepository: LaneRepository,
    private districtRepository: DistrictRepository,
    private datePipe: DatePipe
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      DistrictId: [undefined],
      WardId: [undefined],
      LaneId: [undefined],
      CustomerName: [undefined],
      CodeHS: [undefined],
      Code: [undefined],
      FromDate: [undefined],
      ToDate: [undefined]
    });

    this.getDataDistrict();
    this.getDataWard();
    this.getDataLane();

  }

  async getDataReport() {
    this.loading = true;
    // this.rowData$ = [];
    this.gridApi.setRowData([]);
    this.gridApi.showLoadingOverlay();
    const resp = await this.reportNocRepository.GetDataExport3(this.validateForm.value);
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

  async getDataDistrict() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name';
    paging.query = `ProvinceId=2`;

    const respDistrict = await this.districtRepository.getByPage(paging);
    if (respDistrict.meta?.error_code == 200) {
      this.districts = respDistrict.data;
    }
  }

  async getDataWard() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name,DistrictId';
    paging.query = `ProvinceId=2`;

    const respWard = await this.wardRepository.getByPage(paging);
    if (respWard.meta?.error_code == 200) {
      this.wards = respWard.data;
    }
  }

  async getDataLane() {
    let paging: GetByPageModel = new GetByPageModel();
    paging.page_size = 0;
    paging.select = 'Id,Name,Ward,District';
    paging.query = `Province=2`;

    const respLane = await this.laneRepository.getByPage(paging);
    if (respLane.meta?.error_code == 200) {
      this.lanes = respLane.data;
    }
  }

  onGridReady(params: GridReadyEvent) {
    this.gridApi = params.api;
    // this.gridApi.sizeColumnsToFit();
  }

  changeDistrict() {
    this.validateForm.get('WardId')?.setValue(undefined);
    this.validateForm.get('LaneId')?.setValue(undefined);
  }

  changeWard() {
    this.validateForm.get('LaneId')?.setValue(undefined);
  }

  viewRow(event: any) {
  }
}
