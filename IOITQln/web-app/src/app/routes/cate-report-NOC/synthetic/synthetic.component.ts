import { Component, OnInit } from '@angular/core';
import { TypeHouse } from 'src/app/shared/utils/consts';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { FormBuilder, FormGroup } from '@angular/forms';
import { WardRepository } from 'src/app/infrastructure/repositories/ward.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { DistrictRepository } from 'src/app/infrastructure/repositories/district.repository';
import { ReportNocRepository } from 'src/app/infrastructure/repositories/report-noc.repository';
import { ColDef, GridReadyEvent, ColGroupDef } from 'ag-grid-community';
import { Observable } from 'rxjs';
import { ICellRendererParams, SideBarDef, StatusPanelDef } from 'ag-grid-community';
import 'ag-grid-enterprise';
import { HttpClient } from '@angular/common/http';
import { GridApi, ColumnApi } from 'ag-grid-community';
import { SyntheticItemComponent } from '../synthetic-item/synthetic-item.component';
import { NzModalService } from 'ng-zorro-antd/modal';
import { DatePipe } from '@angular/common';
import { SharedAgGridLoadingOverlayComponent } from 'src/app/shared/components/ag-grid/loading-overlay';

@Component({
  selector: 'app-report-noc-synthetic',
  templateUrl: './synthetic.component.html',
  styles: []
})
export class SyntheticReportComponent implements OnInit {
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
    { headerName: 'Loại biên bản áp dụng', field: 'LoaiBienBan', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Số tt (căn)', field: 'SoTtCan', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Số tt (hộ)', field: 'SoTtHo', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Mã định danh', field: 'MaDinhDanh', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Số nhà', field: 'SoNha', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Địa chỉ', field: 'Duong', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Phường', field: 'Phuong', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Quận', field: 'Quan', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Loại nhà', field: 'LoaiNha', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Cấp nhà/Hạng nhà', field: 'CapNha', onCellClicked: (evt) => { this.viewRow(evt) } },
    {
      headerName: 'Diện tích đất (m2) ',
      children: [
        {
          headerName: 'Diện tích xây dựng', field: 'DienTichXayDung', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Khuôn viên (Tổng diện tích đất)', field: 'KhuonVien', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'DTĐ riêng', field: 'DienTichDatRieng', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'DTĐ chung', field: 'DienTichDatChung', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'DTĐ chung phân bổ', field: 'DienTichDatChungPhanBo', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Diện tích SD (m2) ',
      children: [
        {
          headerName: 'Diện tích SD riêng', field: 'DienTichSuDungRieng', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Diện tích SD chung', field: 'DienTichSuDungChung', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Diện tích SD chung phân bổ', field: 'DienTichSuDungChungPhanBo', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Tổng diện tích SD chung', field: 'TongDienTichSuDungChung', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Tổng diện tích SD', field: 'TongDienTichSuDung', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Thuế phi nông nghiệp',
      children: [
        { headerName: 'Vị trí (V1;V2...)', field: 'ViTri', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Hệ số hẻm (Độ rộng hẻm)', field: 'HeSoHem', onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Hệ số phân bổ', field: 'HeSoPhanBo', onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Đơn giá đất/1m2', field: 'DonGiaDat', onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Thuế suất(0,03%;0,05%...)', field: 'ThueSuat', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Tiền thuế phải nộp từng năm', field: 'TienThuePhaiNopTungNam', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Thông tin chủ hộ',
      children: [
        { headerName: 'Người đại diện ký hợp đồng', field: 'NguoiDaiDien', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'CMND/CCCD/Thẻ quân nhân/Hộ chiếu', field: 'CanCuoc', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Ngày cấp', field: 'NgayCap', valueFormatter: (data: any) => {
            return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        { headerName: 'Nơi cấp', field: 'NoiCap', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Địa chỉ thường trú', field: 'DiaChiThuongTru', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'SĐT', field: 'SoDienThoai', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Thành viên sử dụng nhà', field: 'ThanhVien', onCellClicked: (evt) => { this.viewRow(evt) } }
      ], headerClass: 'text-center'
    },
    { headerName: 'Thời điểm giao nhận nhà ở', field: 'ThoiDiemGiaoNhanNha', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Thời điểm nhà ở được bố trí sử dụng', field: 'ThoiDiemBoTriSuDung', onCellClicked: (evt) => { this.viewRow(evt) } },
    {
      headerName: 'Xác lập SHNN',
      children: [
        { headerName: 'Số quyết định', field: 'SoQuyetDinh', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Ngày ký quyết định', field: 'NgayKyQuyetDinh', valueFormatter: (data: any) => {
            return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Bản vẽ cho thuê',
      children: [
        { headerName: 'Tên bản vẽ', field: 'TenBanVe', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Ngày lập bản vẽ', field: 'NgayLapBanVe', valueFormatter: (data: any) => {
            return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    { headerName: 'Tình trạng nhà', field: 'TinhTrangNha', onCellClicked: (evt) => { this.viewRow(evt) } },
    { headerName: 'Ghi chú', field: 'GhiChu', onCellClicked: (evt) => { this.viewRow(evt) } },
    {
      headerName: 'Kiểm định nhà',
      children: [
        { headerName: 'Cấp nhà', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Số kiểm định', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Ngày kiểm định', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Đơn vị thực hiện', field: '', onCellClicked: (evt) => { this.viewRow(evt) } }
      ], headerClass: 'text-center'
    },
    { headerName: 'Trạng thái hợp đồng', field: 'TrangThaiHopDong', onCellClicked: (evt) => { this.viewRow(evt) } },
    {
      headerName: 'Thông tin tài chính',
      children: [
        { headerName: 'Số giấy xác nhận tiền thuê nhà', field: 'SoGiayXacNhan', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Nợ cũ(Công ích có VAT)', field: 'NoCuCoVat', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    { headerName: 'Số hợp đồng', field: 'SoHopDong', onCellClicked: (evt) => { this.viewRow(evt) } },
    {
      headerName: 'Ngày ký HĐ', field: 'NgayKyHopDong', valueFormatter: (data: any) => {
        return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
      }, onCellClicked: (evt) => { this.viewRow(evt) }
    },
    {
      headerName: 'Giá thuê nhà/tháng có VAT',
      children: [
        {
          headerName: 'Theo xác nhận Công ích', field: 'GiaThueNhaTheoCongIch', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Hợp đồng tạm', field: 'GiaThueNhaHopDongTam', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Chính thức', field: 'GiaThueNhaChinhThuc', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Thông tin bán nhà, đất có VAT',
      children: [
        {
          headerName: 'Giá bán nhà/m2', field: 'GiaBanNha', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Diện tích nhà', field: 'DienTichNha', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Giá bán đất/m2', field: 'GiaBanDat', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Diện tích đất', field: 'DienTichDat', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 2, minimumFractionDigits: 2 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Số tiền miễn giảm(nhà)', field: 'SoTienMienGiamNha', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Số tiền miễn giảm(đất)', field: 'SoTienMienGiamDat', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        {
          headerName: 'Thành tiền', field: 'ThanhTien', valueFormatter: (data: any) => {
            return data.value ? data.value.toLocaleString("de-DE", { maximumFractionDigits: 0, minimumFractionDigits: 0 }) : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        { headerName: 'Trạng thái', field: 'TrangThai', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Số quyết định bán nhà', field: 'SoQuyetDinhBanNha', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Ngày quyết định', field: 'NgayQuyetDinhBanNha', valueFormatter: (data: any) => {
            return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        { headerName: 'Số biên bản thanh lý hợp đồng', field: 'SoBienBanThanhLyHopDong', onCellClicked: (evt) => { this.viewRow(evt) } },
        {
          headerName: 'Ngày thanh lý hợp đồng', field: 'NgayThanhLyHopDong', valueFormatter: (data: any) => {
            return data.value ? (this.datePipe.transform(data.value, "dd/MM/yyyy") ?? "") : "";
          }, onCellClicked: (evt) => { this.viewRow(evt) }
        },
        { headerName: 'Ghi chú', field: 'GhiChuThongTinBan', onCellClicked: (evt) => { this.viewRow(evt) } }
      ], headerClass: 'text-center'
    },
    {
      headerName: 'Thông tin sửa nhà',
      children: [
        { headerName: 'Số lẫn sửa chữa', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Ngày hoàn thành', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Số tiền quyết toán', field: '', onCellClicked: (evt) => { this.viewRow(evt) } },
        { headerName: 'Đơn vị thực hiện', field: '', onCellClicked: (evt) => { this.viewRow(evt) } }
      ], headerClass: 'text-center'
    }
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
    private http: HttpClient,
    private modalSrv: NzModalService,
    private datePipe: DatePipe
  ) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      DistrictId: [undefined],
      WardId: [undefined],
      LaneId: [undefined],
      Code: [undefined],
      CustomerName: [undefined],
      IdentityCode: [undefined],
      HasInfo: [undefined]
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

    let data = this.validateForm.value;
    data.HasSaleInfo = data.HasInfo == "1" ? true : undefined;
    data.HasRentInfo = data.HasInfo == "2" ? true : undefined;
    const resp = await this.reportNocRepository.GetSyntheticReportNoc(data);
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
    let data = event.data;
    this.modalSrv.create({
      nzTitle: `Thông tin chi tiết`,
      nzContent: SyntheticItemComponent,
      nzAutofocus: null,
      nzComponentParams: {
        data: data,
      },
      nzWidth: "600px"
    });
  }
}
