import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { Md167ReceiptRepository } from 'src/app/infrastructure/repositories/md167-receipt.repository';
import { Md167LandPriceRepository } from 'src/app/infrastructure/repositories/md167landprice.repository';
import { Md167HouseRepository } from 'src/app/infrastructure/repositories/md167house.repository';
import { Md167KiosRepository } from 'src/app/infrastructure/repositories/md167kios.repository';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import QueryModel from 'src/app/core/models/query-model';
import { STChange, STColumn, STComponent } from '@delon/abc/st';
import { ImportHistoryRepository } from 'src/app/infrastructure/repositories/import-history.repository';
import { Md167ContractRepository } from 'src/app/infrastructure/repositories/md167-contract.repository';
import { SalaryRepository } from 'src/app/infrastructure/repositories/salary.repository';
import { TimeCoefficientRepository } from 'src/app/infrastructure/repositories/time-coeficient.repository';
import { ConversionRepository } from 'src/app/infrastructure/repositories/conversion.repository';
import { DefaultCoefficientRepository } from 'src/app/infrastructure/repositories/default-coefficient.repositories';
import { DiscountCoefficientRepository } from 'src/app/infrastructure/repositories/discount-coefficient.repositories';
import { RentingPriceRepository } from 'src/app/infrastructure/repositories/renting-price.repository';
import { DepartmentRepository } from 'src/app/infrastructure/repositories/department.repository';
import { PositionRepository } from 'src/app/infrastructure/repositories/position.repository';
import { LaneRepository } from 'src/app/infrastructure/repositories/lane.repository';
import { LandPriceRepository } from 'src/app/infrastructure/repositories/land-price.repository';
import { LandType } from '../../utils/consts';
import { RentFileRepository } from 'src/app/infrastructure/repositories/rent-fille.repositories';
import { IngredientsPriceRepository } from 'src/app/infrastructure/repositories/ingredients-price.repository';
import { OriginalPriceAndTaxRepository } from 'src/app/infrastructure/repositories/original-price-and-tax.repository';
import { ProfitValueRepository } from 'src/app/infrastructure/repositories/profit-value.repository';
import { AnnualInstallmentRepository } from 'src/app/infrastructure/repositories/annual-installment.repository';
import { ResettlementApartmentRepository } from 'src/app/infrastructure/repositories/resettlement-apartment.repository';
import { LandRepository } from 'src/app/infrastructure/repositories/land.repository';
import { BlockHouseRepository } from 'src/app/infrastructure/repositories/block-house.repository';
import { FloorTdcRepository } from 'src/app/infrastructure/repositories/floor-tdc.repository';
import { ApartmentTdcRepository } from 'src/app/infrastructure/repositories/apartment-tdc.repository';
import { PlatformTdcRepository } from 'src/app/infrastructure/repositories/platform-tdc.repository';
import { TDCProjectRepository } from 'src/app/infrastructure/repositories/tdcproject.repository';
import { TdcCustomerRepository } from 'src/app/infrastructure/repositories/tdcCustomer.repository';

@Component({
  selector: 'app-shared-import-excel',
  templateUrl: './import-excel.component.html',
  styles: []
})
export class SharedImportExcelComponent implements OnInit {
  @Input() importHistoryType: ImportHistoryTypeEnum;
  ImportHistoryTypeEnum = ImportHistoryTypeEnum;

  data: any[] = [];
  loading = false;

  paging: GetByPageModel = new GetByPageModel();
  query: QueryModel = new QueryModel();

  PlotType = {
    UnknowLand: 3,
    LandApartment: 1,
    LandPlatform: 2
  };

  columns: STColumn[] = [
    // { title: '', index: 'Id', type: 'checkbox' },
    { title: 'Stt', render: 'no-column', width: 60, className: 'text-center' },
    { title: 'Người thực hiện', index: 'CreatedBy', width: 200 },
    { title: 'Thời gian thực hiện', index: 'CreatedAt', width: 150, className: 'text-center', dateFormat: 'dd/MM/yyyy HH:mm', type: 'date' },
    {
      title: 'File import',
      width: '150px',
      className: 'text-center',
      buttons: [
        {
          icon: 'download',
          click: record => this.downloadFileExcelHistory(record.Id)
        }
      ]
    },
    {
      title: 'Chi tiết',
      width: '150px',
      className: 'text-center',
      buttons: [
        {
          icon: 'eye',
          click: record => { this.dataImportDetail = record.Data }
        }
      ]
    }
  ];

  dataImport: any[] = [];
  dataImportDetail: any[] = [];

  constructor(private md167ReceiptRepository: Md167ReceiptRepository, private drawerRef: NzDrawerRef<string>, private uploadRepository: UploadRepository,
    private importHistoryRepository: ImportHistoryRepository,
    private md167LandPriceRepository: Md167LandPriceRepository,
    private md167HouseRepository: Md167HouseRepository,
    private md167KiosRepository: Md167KiosRepository,
    private md167ContractRepository: Md167ContractRepository,
    private salaryRepository: SalaryRepository,
    private timeCoefficientRepository: TimeCoefficientRepository,
    private conversionRepository: ConversionRepository,
    private defaultCoefficientRepository: DefaultCoefficientRepository,
    private discountCoefficientRepository: DiscountCoefficientRepository,
    private rentingPriceRepository: RentingPriceRepository,
    private departmentRepository: DepartmentRepository,
    private positionRepository: PositionRepository,
    private laneRepository: LaneRepository,
    private landPriceRepository: LandPriceRepository,
    private rentFileRepository: RentFileRepository,
    private ingredientsPriceRepository: IngredientsPriceRepository,
    private originalPriceAndTaxRepository: OriginalPriceAndTaxRepository,
    private profitValueRepository: ProfitValueRepository,
    private annualInstallmentRepository: AnnualInstallmentRepository,
    private resettlementApartmentRepository: ResettlementApartmentRepository,
    private landrepository: LandRepository,
    private blockHouseRepository: BlockHouseRepository,
    private floorTdcRepository: FloorTdcRepository,
    private apartmentTdcRepository: ApartmentTdcRepository,
    private platformTdcRepository: PlatformTdcRepository,
    private tDCProjectRepository: TDCProjectRepository,
    private tdcCustomerRepository: TdcCustomerRepository,
  ) { }

  ngOnInit(): void {
    this.getData();
  }

  getPlotTypeName(plotTypeValue: number): string {
    switch (plotTypeValue) {
      case this.PlotType.UnknowLand:
        return 'Chưa xác định';
      case this.PlotType.LandApartment:
        return 'Lô chung cư';
      case this.PlotType.LandPlatform:
        return 'Lô nền đất';
      default:
        return 'lỗi';
    }
  }

  async getData() {
    this.paging.page_size = 10;
    const resp = await this.importHistoryRepository.getByPage(this.paging, this.importHistoryType);
    if (resp.meta?.error_code == 200) {
      this.dataImport = resp.data;
      this.paging.item_count = resp.metadata;
    }
  }

  tableRefChange(e: STChange): void {
    switch (e.type) {
      case 'pi':
        this.paging.page = e.pi;
        this.getData();
        break;
      case 'dblClick':
        break;
      case 'checkbox':
        break;
      case 'sort':
        this.paging.order_by = e.sort?.value ? `${e.sort?.column?.index?.toString()} ${e.sort?.value.replace("end", "")}` : new GetByPageModel().order_by;
        this.getData();
        break;
      default:
        break;
    }
  }

  close(): void {
    this.drawerRef.close();
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.data = [];
    this.handleChange(file);
    return false;
  };

  async handleChange(file: any) {
    const formData = new FormData();
    formData.append(file.name, file);

    this.loading = true;
    switch (this.importHistoryType) {
      case ImportHistoryTypeEnum.Md167Receipt:
        const resp = await this.md167ReceiptRepository.importDataExcel(formData);
        this.data = resp.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Md167Landprice:
        const resp_landprice = await this.md167LandPriceRepository.importDataExcel(formData);
        this.data = resp_landprice.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Md167House:
        const resp_house = await this.md167HouseRepository.importDataExcel(formData);
        this.data = resp_house.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Md167Kios:
        const resp_kios = await this.md167KiosRepository.ImportDataExcelKios(formData);
        this.data = resp_kios.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Md167MainContract:
        const resp_contract = await this.md167ContractRepository.importDataExcel(formData, this.importHistoryType);
        this.data = resp_contract.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Md167ExtraContract:
        const resp_contract_extra = await this.md167ContractRepository.importDataExcel(formData, this.importHistoryType);
        this.data = resp_contract_extra.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocSalary:
        const resp_salary = await this.salaryRepository.importDataExcel(formData);
        this.data = resp_salary.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocCoefficient:
        const resp_coefficient = await this.timeCoefficientRepository.importDataExcel(formData);
        this.data = resp_coefficient.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocConversion:
        const resp_conversion = await this.conversionRepository.importDataExcel(formData);
        this.data = resp_conversion.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocDefaultCoeficient:
        const resp_default = await this.defaultCoefficientRepository.importDataExcel(formData);
        this.data = resp_default.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocDiscountCoefficient:
        const resp_dis = await this.discountCoefficientRepository.importDataExcel(formData);
        this.data = resp_dis.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.NocRentingPrice:
        const resp_ren = await this.rentingPriceRepository.importDataExcel(formData);
        this.data = resp_ren.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Common_Department:
        const resp_department = await this.departmentRepository.importDataExcel(formData);
        this.data = resp_department.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Common_Position:
        const resp_pos = await this.positionRepository.importDataExcel(formData);
        this.data = resp_pos.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Common_Lane:
        const resp_lane = await this.laneRepository.importDataExcel(formData);
        this.data = resp_lane.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Noc_Landprice:
        const resp_lp = await this.landPriceRepository.importDataExcel(formData);
        this.data = resp_lp.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Noc_Contract_Rent_Receipt:
        const resp_crr = await this.rentFileRepository.importDataExcel(formData);
        this.data = resp_crr.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.Noc_Contract_Rent:
        const resp_ncr = await this.rentFileRepository.importContractDataExcel(formData);
        this.data = resp_ncr.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcIngredientsPrice:
        const resp_ingrePrice = await this.ingredientsPriceRepository.importDataExcel(formData);
        this.data = resp_ingrePrice.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcOriginalPriceAndTax:
        const resp_original = await this.originalPriceAndTaxRepository.importDataExcel(formData);
        this.data = resp_original.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcProfitValue:
        const resp_profit = await this.profitValueRepository.importDataExcel(formData);
        this.data = resp_profit.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcInstallmentRate:
        const resp_installment = await this.annualInstallmentRepository.importDataExcel(formData);
        this.data = resp_installment.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcResettlement:
        const resp_resettlement = await this.resettlementApartmentRepository.importDataExcel(formData);
        this.data = resp_resettlement.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcLand:
        const resp_land = await this.landrepository.importDataExcel(formData);
        this.data = resp_land.data;
        this.getData();
        break;  
      case ImportHistoryTypeEnum.TdcBlockHouse:
        const resp_blockhouse = await this.blockHouseRepository.importDataExcel(formData);
        this.data = resp_blockhouse.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcFloor:
        const resp_floortdc = await this.floorTdcRepository.importDataExcel(formData);
        this.data = resp_floortdc.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcApartment:
        const resp_apartmenttdc = await this.apartmentTdcRepository.importDataExcel(formData);
        this.data = resp_apartmenttdc.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcPlatform:
        const resp_platformtdc = await this.platformTdcRepository.importDataExcel(formData);
        this.data = resp_platformtdc.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcProject:
        const resp_project = await this.tDCProjectRepository.importDataExcel(formData);
        this.data = resp_project.data;
        this.getData();
        break;
      case ImportHistoryTypeEnum.TdcCustomer:
        const resp_customer = await this.tdcCustomerRepository.importDataExcel(formData);
        this.data = resp_customer.data;
        this.getData();
        break;
      default:
        break;
    }

    this.loading = false;
  }
  

  downloadTemplate() {
    this.uploadRepository.downloadFileExcelTemplate(this.importHistoryType);
  }

  downloadFileExcelHistory(id: number) {
    this.uploadRepository.downloadFileExcelHistory(id);
  }

  getLandName(id: number) {
    return LandType.find(s => s.Id == id)?.Name;
  }

}
