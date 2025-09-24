export enum RowStatus {
  AddNew,
  Update
}

export enum EntityStatus {
  INIT = 0,
  NORMAL = 1,
  TEMP = 10,
  LOCK = 98,
  DELETE = 99
}

export enum TypeAttributeCode {
  NHOM_BIEU_MAU = 'NHOM_BIEU_MAU',
  TINH_TRANG_QUAN_LY = 'TINH_TRANG_QUAN_LY',
  LOAI_CAN = 'LOAI_CAN',
  LOAI_VAN_BAN_XAC_MINH = 'LOAI_VAN_BAN_XAC_MINH',
  TYPE_BLOCK = 'TYPE_BLOCK', //Loại nhà
  LOAI_QUYET_DINH_TDC = 'LOAI_QUYET_DINH_TDC',
  TRANGTHAI_HOPDONG_THUE_NOC = 'CONTRACT_RENT_NOC_STATUS'
}

export enum TypeCoeFFicient {
  SALARY = 1,
  TIME_LAYOUT = 2,
  VAT = 3
}

export enum TypeDecree {
  NGHIDINH = 1,
  THONGTU = 2
}

export enum DecreeEnum {
  ND_99 = 99,
  ND_34 = 34,
  ND_61 = 61,
  SPECIAL = 9999
}

export enum TermApplyEnum {
  KHOAN_1_DIEU_34 = 134,
  KHOAN_2_DIEU_34 = 234,
  DIEU_35 = 35,
  DIEU_65 = 65,
  DIEU_70 = 70,
  DIEU_71 = 71,
  DIEU_7 = 61
}

//Trạng thái Mã định danh
export enum CodeStatusEnum {
  CHUA_TON_TAI = 1,
  DA_TON_TAI = 2,
  DA_CAP_NHAT = 3
}

//
export enum TypeBlockEntityEnum {
  BLOCK_NORMAL = 1,
  BLOCK_RENT = 2
}

export enum TypeReportApplyEnum {
  NHA_HO_CHUNG = 1,
  NHA_RIENG_LE = 2,
  NHA_CHUNG_CU = 3,
  BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG = 4,
  BAN_PHAN_DIEN_TICH_LIEN_KE = 5
}

export enum TypeApartmentEntityEnum {
  APARTMENT_NORMAL = 1,
  APARTMENT_RENT = 2
}

export enum TypeCaseApply_34Enum {
  KHOAN_1 = 1,
  KHOAN_2 = 2
}

export enum TypeApartmentDetailEnum {
  BLOCK = 1,
  APARTMENT = 2
}

export enum TypeApartmentLandDetailEnum {
  BLOCK = 1,
  APARTMENT = 2
}

export enum LandPriceType {
  NOC = 1,
  MD167 = 2
}

//Loại giá của hợp đồng 167
export enum TypePriceContract167Enum {
  GIA_NIEM_YET = 1,
  DAU_GIA = 2
}

//Thơi gian thuê - hợp đồng 167
export enum RentalPeriodContract167Enum {
  TAM_BO_TRI = 1,
  THUE_1_NAM = 2,
  THUE_5_NAM = 3,
  KHAC = 4
}

//Mục đích thuê - hợp đồng 167
export enum RentalPurposeContract167Enum {
  KINH_DOANH_DV = 1,
  CO_SO_SX = 2,
  KHO_BAI = 3,
  KHAC = 4
}

//Kỳ thanh toán - hợp đồng 167
export enum PaymentPeriodContract167Enum {
  THANG = 1,
  QUY = 2
}

//Trạng thái hợp đồng 167
export enum ContractStatus167Enum {
  CON_HIEU_LUC = 1,
  HET_HIEU_LUC = 2
}

//Loại hợp đồng 167
export enum Contract167TypeEnum {
  MAIN = 1, //hợp đồng chính
  EXTRA = 2 //phụ lục hợp đồng
}

//Loại nhà 167
export enum TypeHouse167 {
  Kios = 3,
  House = 1,
  Apartment = 2
}

export enum TypeContractTDC {
  MAIN = 1, //hợp đồng chính
  EXTRA = 2 //phụ lục hợp đồng
}

export enum ImportHistoryTypeEnum {
  Md167Receipt = 1,           //Import phiếu thu cho hợp đồng nhà ở 167
  Md167Landprice = 2,         //Import bảng giá đất cho nhà ở 167
  Md167House = 3,             //Import Thông tin cơ sở nhà đất
  Md167Kios = 4,              //Import Kios cho nhà ở 167
  Md167MainContract = 5,      //Import danh sách hợp đồng
  Md167ExtraContract = 6,     //Import danh sách phụ lục hợp đồng
  NocSalary = 7,              //Import hệ số lương
  NocCoefficient = 8,         //Import hệ số thời gian
  NocConversion = 9,          //Import hệ số quy đổi
  NocDefaultCoeficient = 10,  //Import hệ số mặc định
  NocDiscountCoefficient = 11,//Import hệ số vị trí
  NocRentingPrice = 12,       //Import bảng giá thuê
  NocBlock = 13,              //Import Căn nhà
  NocRentBlock = 14,          //Import căn nhà thuê-chưa làm
  NocDecreeType2 = 30,        //Import văn bản pháp luật liên quan
  Noc_Ct_MainTexture = 31,    //Import Hiện trạng kết cấu chính - Noc
  Noc_Ratio_MainTexture = 32,                 //Import Tỷ lệ giá trị kết cấu chính - Noc
  Noc_Construction_price = 33,                //Import Chỉ số giá xây dựng công trình - Noc
  Noc_Salary_Coefficient = 34,                //Import Lương cơ bản - Noc
  Noc_Deduction_Coefficient = 35,             //Import Hệ số miễn giảm tiền nhà - Noc
  Noc_Area_Correction_Coefficient = 36,       //Import Hệ số điều chỉnh vùng - Noc
  Noc_No2_LandPrice = 37,                     //Import Bảng số đất số 2 - Noc
  Noc_Land_Special_Coefficient = 38,          //Import Hệ số khu đất, thửa đất có hình dáng đặc biệt - Noc
  Noc_Position_Coefficient = 39,              //Hệ số vị trí - Noc
  Noc_K_LandPrice_Correction_Coefficient = 40,    //Hệ số K điều chỉnh giá đất - Noc
  Noc_Deduction_Land_Money = 41,                  //Tiền đất miễn giảm - Noc
  Noc_Customer = 42,                              //Khách hàng - Noc
  Noc_PriceList = 43,                              //Bảng giá nhà ở
  Noc_GeneralDataImport = 44,                     //Import dữ liệu nhà ở cũ từ file excel tổng hợp
  Noc_Landprice = 45,                             //Bảng giá đất nhà ở cũ
  Noc_Contract_Rent_Receipt = 46,                 //Phiếu thu - Hợp đồng thuê NOC
  Noc_Contract_Rent = 47,                         //Hợp đồng thuê NOC
  Common_Department = 70,                             //Phòng ban - danh mục chung
  Common_Position = 71,                               //Chức danh - danh mục chung
  Common_Lane = 72,                                   //Đường - danh mục chung
  TdcIngredientsPrice = 73,      //Import thành phần giá bán cấu thành TDC
  TdcOriginalPriceAndTax = 74,   //Import thành phần gái-gốc thuế phí TDC
  TdcProfitValue = 75,           //Import hệ số lãi phạt thuê TDC
  TdcInstallmentRate = 76,       //Import lãi suất trả góp hàng năm
  TdcResettlement = 77,          //Import chung cư
  TdcLand = 78,                  //Import lô 
  TdcBlockHouse = 79,            //Import khối nhà tdc
  TdcFloor = 80,                 //Import tầng tdc
  TdcApartment = 81,             //Import căn tdc
  TdcPlatform = 82,              //Import nển tdc
  TdcProject = 83,               //Import dự án tdc
  TdcCustomer = 84,              //Import khách hàng tdc
  Tdc_ApartmentDataImport = 85,//Import danh sách căn hộ trống tiếp nhận và quản lý
  Tdc_PlatformDataImport = 86, //Import danh sách nền đất trống tiếp nhận và quản lý
}

export enum AccessKey {
  SPECIAL_NOC_SELL_DEBT_BACK_STATUS_PAYMENT = "SPECIAL_NOC_SELL_DEBT_BACK_STATUS_PAYMENT",
  DEBT_RENT_CONTRACT_NOC = "DEBT_RENT_CONTRACT_NOC",
  IMPORT_RENT_CONTRACT_NOC = "IMPORT_RENT_CONTRACT_NOC",
  EXPORT_RECEIPT_CONTRACT_NOC = "EXPORT_RECEIPT_CONTRACT_NOC",
  IMPORT_RECEIPT_CONTRACT_NOC = "IMPORT_RECEIPT_CONTRACT_NOC",
  DECREE_TYPE2_MANAGEMENT = "DECREE_TYPE2_MANAGEMENT",
  LAND_PRICE_MANAGEMENT = "LAND_PRICE_MANAGEMENT",
  CUSTOMER_MANAGEMENT = "CUSTOMER_MANAGEMENT",
  SALARY_MANAGEMENT = "SALARY_MANAGEMENT",
  RENT_FILE = "RENT_FILE",
  COEFFICIENT = "COEFFICIENT",
  CONVERSION = "CONVERSION",
  DEFAULTCOEFFICIENT = "DEFAULTCOEFFICIENT",
  RENT_BLOCK = "RENT_BLOCK",
  RENT_APARTMENT = "RENT_APARTMENT",
  DISCOUNT_COFFICIENT = "DISCOUNT_COFFICIENT",
  PRICING = "PRICING",
  BLOCK_MANAGEMENT = "BLOCK_MANAGEMENT",
  APARTMENT_MANAGEMENT = "APARTMENT_MANAGEMENT",
  TYPE_BLOCK_MANAGEMENT = "TYPE_BLOCK_MANAGEMENT",
  FLOOR_MANAGEMENT = "FLOOR_MANAGEMENT",
  AREA_MANAGEMENT = "AREA_MANAGEMENT",
  CURRENTSTATE_MAINTEXTURE_MANAGEMENT = "CURRENTSTATE_MAINTEXTURE_MANAGEMENT",
  RATIO_MAINTEXTURE_MANAGEMENT = "RATIO_MAINTEXTURE_MANAGEMENT",
  CONSTRUCTION_PRICE_MANAGEMENT = "CONSTRUCTION_PRICE_MANAGEMENT",
  PRICELIST_MANAGEMENT = "PRICELIST_MANAGEMENT",
  USE_VALUE_COEFFICIENT_MANAGEMENT = "USE_VALUE_COEFFICIENT_MANAGEMENT",
  SALARY_COEFFICIENT_MANAGEMENT = "SALARY_COEFFICIENT_MANAGEMENT",
  DEDUCTION_COEFFICIENT_MANAGEMENT = "DEDUCTION_COEFFICIENT_MANAGEMENT",
  INVESTMENT_RATE_MANAGEMENT = "INVESTMENT_RATE_MANAGEMENT",
  AREA_CORRECTION_COEFFICIENT_MANAGEMENT = "AREA_CORRECTION_COEFFICIENT_MANAGEMENT",
  NO2_LAND_PRICE_MANAGEMENT = "NO2_LAND_PRICE_MANAGEMENT",
  LAND_SPECIAL_COEFFICIENT = "LAND_SPECIAL_COEFFICIENT",
  DISTRIBUTION_FLOOR_COEFFICIENT_MANAGEMENT = "DISTRIBUTION_FLOOR_COEFFICIENT_MANAGEMENT",
  POSITION_COEFFICIENT_MANAGEMENT = "POSITION_COEFFICIENT_MANAGEMENT",
  LAND_PRICE_CORRECTION_COEFFICIENT_MANAGEMENT = "LAND_PRICE_CORRECTION_COEFFICIENT_MANAGEMENT",
  DEDUCTION_LAND_MONEY_MANAGEMENT = "DEDUCTION_LAND_MONEY_MANAGEMENT",
  LANDSCAPE_LIMIT_MANAGEMENT = "LANDSCAPE_LIMIT_MANAGEMENT",
  RENTING_PRICE_MANAGEMENT = "RENTING_PRICE_MANAGEMENT",
  MD167_DASHBOARD = "MD167_DASHBOARD"
}

export enum TypeEditHistoryEnum {
  BLOCK = 1,
  APARTMENT = 2,
  RENT_CONTRACT = 3,
  SELL_PRICING = 4
}

export enum ModuleSystem {
  NOC = 1,
  NTDC = 2,
  N167 = 3
}

export enum TypeSignalRNotify {
  LOG_OUT = 1,
  NOTIFY = 2,
  RE_LOGIN = 3
}