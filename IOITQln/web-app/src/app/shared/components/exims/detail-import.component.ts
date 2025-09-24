import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CellClickedEvent, ColDef, ColumnApi, GridApi, GridOptions, GridReadyEvent } from 'ag-grid-community';
import { NzModalService } from 'ng-zorro-antd/modal';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { DeleteCellRenderer } from 'src/app/shared/custom_editor/delete-cell-renderer';
import { defaultColDefBase, gridOptionsBase } from 'src/app/shared/utils/grid_config';
import { AutocompleAgGrid } from '../../custom_editor/autocomplete/autocomplete-editor';
import { currencyFormatter } from 'src/app/shared/utils/common';

@Component({
  selector: 'app-detail-import-exim',
  templateUrl: './detail-import.component.html',
  styles: []
})
export class DetailImportEximComponent implements OnInit {
  @Input() rowData: any;
  @Output() changeEmitter: EventEmitter<any> = new EventEmitter<any>();

  sickCategory: any = [];
  gridStyle: any;

  gridApi!: GridApi;
  columnApi!: ColumnApi;

  paging: GetByPageModel = new GetByPageModel();

  defaultColDef: ColDef = {
    ...defaultColDefBase
  };

  columnDefs: ColDef[] = [
    {
      field: 'MaHh',
      headerName: 'Mã hàng hóa',
      cellEditor: AutocompleAgGrid,
      cellEditorParams: {
        onSearch: (keyword: string) => this.getServiceProduct(keyword),
        onSelect: this.onSelectAutoComplete.bind(this),
        checkExist: this.checkExist.bind(this),
        keyField: 'Id',
        labelField: 'TenDichVu'
      },
      editable: prams => !prams.data.keyCommand,
      flex: 1,
      cellClass: params => {
        if (!params.data.keyCommand) return 'align-center';
        return 'rowCommand';
      },
      valueGetter: params => {
        if (params.data.keyCommand) return '+ Thêm dòng mới';
        return params.data[params.colDef.field!];
      },
      colSpan: params => {
        if (params.data.keyCommand) return 12;
        return 1;
      }
    },
    {
      field: 'TenHh',
      headerName: 'Tên hàng hóa',
      flex: 3,
      editable: true
    },
    // { field: 'NoiSanXuat', headerName: 'Nơi sản xuất', flex: 3, editable: true },
    { field: 'SoDk', headerName: 'Số đăng ký', flex: 2, editable: true },
    { field: 'LoSx', headerName: 'Số lô', flex: 2, editable: true },
    { field: 'GoiThau', headerName: 'Gói thầu', flex: 2, editable: true },
    { field: 'HanSd', headerName: 'Hạn sử dụng', flex: 2, editable: true },
    { field: 'DvNhap', headerName: 'Đơn vị tính', flex: 2, editable: true },
    {
      field: 'SoLuong',
      headerName: 'SL',
      flex: 1,
      editable: true,
      cellClass: ['text-right'],
      headerClass: ['text-right']
    },
    {
      field: 'DonGia',
      headerName: 'Đơn giá',
      flex: 2,
      editable: true,
      valueFormatter: params => currencyFormatter(params.value),
      cellClass: ['text-right'],
      headerClass: ['text-right']
    },
    {
      field: 'ThanhTien',
      headerName: 'Thành tiền',
      flex: 2,
      editable: false,
      cellClass: ['text-right'],
      headerClass: ['text-right'],
      valueFormatter: params => currencyFormatter(params.value)
    },
    {
      width: 40,
      suppressMenu: true,
      editable: false,
      cellRendererParams: {
        onDeleteRow: this.onDeleteRow.bind(this)
      },
      suppressPaste: true,
      cellRenderer: DeleteCellRenderer,
      singleClickEdit: false,
      cellClass: ['no-focus', 'text-center']
    }
  ];

  gridOptions: GridOptions = {
    ...gridOptionsBase,
    columnDefs: this.columnDefs,
    defaultColDef: this.defaultColDef,
    onGridReady: (event: GridReadyEvent) => this.onGridReady(event),
    // onCellValueChanged: (params) => this.onCellValueChanged(params),
    onCellClicked: params => this.onCellClicked(params)
  };

  constructor(
    private modalSrv: NzModalService
  ) {}

  ngOnInit(): void {
    this.gridStyle = { height: `${window.innerHeight / 4}px` };
    this.rowData = [...(this.rowData ?? []), { keyCommand: '#addNewRow' }];
  }

  onGridReady = (params: GridReadyEvent) => {
    this.gridApi = params.api;
    this.columnApi = params.columnApi;
    // this.columnApi.sizeColumnsToFit(window.innerWidth - window.innerWidth / 4);
  };

  async getServiceProduct(query: string) {
    //let _paging = new GetByPageModel();
    //_paging.page_size = 5;
    //_paging.query = ` VietTat.Contains("${query}") OR TenDichVu.Contains("${query}")`;

    //let resp = await this.serviceProductRepository.getByPage(_paging);
    //return resp.data;
  }

  checkExist(selectedObj: any) {
    let exist = this.rowData.find((item: any) => item.MaHh == selectedObj.MaHh);

    return exist;
  }

  onSelectAutoComplete(rowIndex: number, selectedObj: any) {
    let newRowData = [...this.rowData];
    let newObj = {
      MaHh: selectedObj.Id,
      TenHh: selectedObj.TenDichVu,
      SoDk: selectedObj.SoDangKy,
      DvNhap: selectedObj.DonViTinh,
      DonGia: selectedObj.DonGiaVp
      // LoSx: (this.rowData.length + 1),
      // GoiThau: (this.rowData.length + 1),
      // HanSd: (this.rowData.length + 1),
    };

    newRowData[rowIndex] = { ...newRowData[rowIndex], ...newObj };
    this.rowData = newRowData;

    return false;
    //TODO: check trùng
  }

  onCellClicked = (event: CellClickedEvent) => {
    if (event.data.keyCommand === '#addNewRow') {
      this.handleAdd();
    }
  };

  handleAdd = () => {
    console.log('handleAdd');

    // let hasBenhChinh = this.rowData.find((item: any) => item.ChanBenh == 'Bệnh chính');

    const newRow = {
      // Id: Math.floor(Math.random() * 10),
      // MaBenh: '',
      // TenBenh: '',
      // ChanBenh: !hasBenhChinh ? 'Bệnh chính' : 'Bệnh kèm theo'
    };

    const newRowData = [...this.rowData];
    newRowData.splice(this.rowData.length - 1, 0, newRow);
    this.rowData = newRowData;

    const rowIndex = this.rowData.length - 2;
    // this.gridApi?.ensureIndexVisible(rowIndex + 1);
    this.gridApi.startEditingCell({
      rowIndex: rowIndex,
      colKey: 'MaHh'
    });
  };

  onDeleteRow(data: any) {
    if (data) {
      this.rowData = this.rowData.filter((item: any) => item !== data);
      this.changeEmitter.emit(true);
    }
  }

  submitForm() {
    return this.rowData.filter((item: any) => !item.keyCommand);
  }

  onCellValueChanged(evt: any) {
    if (evt.colDef.field == 'SoLuong' || evt.colDef.field == 'DonGia') {
      if (
        this.rowData[evt.rowIndex].SoLuong != undefined &&
        this.rowData[evt.rowIndex].DonGia != undefined &&
        this.rowData[evt.rowIndex].SoLuong != '' &&
        this.rowData[evt.rowIndex].DonGia != ''
      ) {
        evt.data.ThanhTien = (parseInt(this.rowData[evt.rowIndex].SoLuong) * parseInt(this.rowData[evt.rowIndex].DonGia)).toString();
      } else {
        evt.data.ThanhTien = '0';
      }

      this.gridApi.setRowData(this.rowData);
      this.changeEmitter.emit(true);
    }
  }
}
