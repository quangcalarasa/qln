using System;
using System.Collections.Generic;
using System.Text;

namespace IOITQln.Common.Enums
{
    public class AppEnums
    {
        public enum EntityStatus
        {
            /// <summary>
            /// All
            /// </summary>
            NA = 0,
            NORMAL = 1,
            OK = 2,
            NOT_OK = 3,
            SAP_DELETED = 9,
            TEMP = 10,
            LOCK = 98,
            DELETED = 99,
        }

        public enum CacheDataTypes
        {
            ByteArray = 0, // Can convert to byte array
            Json = 1
        }
        public enum MD167CustomerType
        {
            KHACK_CONG_TY = 1,
            KHACH_CA_NHAN = 2
        }
        public enum MD167ServiceType
        {
            DICH_VU_QL = 1,
            DICH_VU_DIEN = 3,
            DICH_VU_NUOC = 4,
            DICH_VU_XE = 5,
            DICH_VU_KHAC = 6
        }

        public enum Action
        {
            VIEW = 0,
            CREATE = 1,
            UPDATE = 2,
            DELETED = 3,
            IMPORT = 4,
            EXPORT = 5,
            PRINT = 6,
            EDIT_ANOTHER_USER = 7,
            MENU = 8
        }

        public enum TypeFunction    // Phân quyền chức năng với người dùng và nhóm quyền
        {
            FUNCTION_USER = 1, // Người dùng - Chức năng
            FUNCTION_ROLE = 2,    // Nhóm quyền - Chức năng
        }

        public enum TypeReportApply     //Loại biên bản áp dụng
        {
            NHA_HO_CHUNG = 1,
            NHA_RIENG_LE = 2,
            NHA_CHUNG_CU = 3,
            BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG = 4,
            BAN_PHAN_DIEN_TICH_LIEN_KE = 5
        }

        public enum LocationResidentialLand     //Vị trí đất ở
        {
            FIRST = 1,
            SECOND = 2,
            THIRD = 3,
            FOURTH = 4
        }

        public enum LevelAlley          //Cấp hẻm
        {
            LEVEL_1 = 1,    //Hẻm cấp 1
            LEVEL_2 = 2,    //hẻm cấp 2
            LEVEL_3 = 3     //hẻm cấp còn lại
        }

        public enum TypeMainTexTure        //Loại kết cấu chính
        {
            MONG = 1,
            KHUNG_COT = 2,
            TUONG = 3,
            NEN_SAN = 4,
            KHUNG_COT_DO_MAI = 5,
            MAI = 6
        }

        public enum TypeDecree              //Loại nghị định
        {
            NGHI_DINH = 1,
            THONG_TU_VAN_BAN = 2
        }

        public enum TypePile            //Loại móng cọc
        {
            L_NHOHON_HOAC_BANG_15M = 1,
            L_LONHON_15M = 2
        }

        //public enum TypeFloor           //Loại tầng
        //{
        //    TANG_1 = 1,
        //    TANG_2 = 2,
        //    TANG_3 = 3,
        //    TANG_4 = 4,
        //    TANG_5 = 5,
        //    TANG_6_TRO_LEN = 6                  //Tầng 6 trở lên
        //}

        public enum NumberFloor         //Số tầng
        {
            HAI_TANG = 2,
            BA_TANG = 3,
            BON_TANG = 4,
            NAM_TANG_TRO_LEN = 5        //5 tầng trở lên
        }

        public enum TypeSex     //Giới tính
        {
            MALE = 1,
            FEMALE = 2,
            OTHER = 3
        }

        //Vị trí đặc biệt của khu đất
        public enum TypeLandSpecial
        {
            TYPE_1 = 1, //Đoạn đường nằm hai bên dạ cầu (song song cầu)
            TYPE_2 = 2, //Đoạn đường nằm hai bên cầu vượt (song song cầu)
            TYPE_3 = 3, //Nằm trong hành lang bảo vệ của đường điện cao thế
            TYPE_4 = 4, //Đường nhánh dẫn lên cầu vượt
            TYPE_5 = 5 //Cách lề đường bằng một con kênh, rạch không được san lấp
        }

        public enum TypeBlockMaintextureRate
        {
            BLOCK = 1,
            APARTMENT = 2
        }

        //Điều khoản áp dụng
        public enum TermApply
        {
            KHOAN_1_DIEU_34 = 134,
            KHOAN_2_DIEU_34 = 234,
            DIEU_35 = 35,
            DIEU_65 = 65,
            DIEU_70 = 70,
            DIEU_71 = 71,
            DIEU_7 = 61
        }

        //Phân hệ chức năng
        public enum SubSystem
        {
            COMMON = 1,     //Phân hệ chung
            NOC = 2,        //Phân hệ Nhà ở cũ
            TDC = 3         //Phân hệ Nhà tái định cư
        }

        //Nghị định
        public enum DecreeEnum
        {
            ND_CP_99_2015 = 99,
            ND_CP_34_2013 = 34,
            ND_CP_61 = 61
        }

        //Loại Bảng map Nhiều nghị định
        public enum TypeDecreeMapping
        {
            BLOCK = 1           //Map nghị định và căn nhà
        }

        public enum TypeCaseApply_34
        {
            KHOAN_1 = 1,
            KHOAN_2 = 2
        }

        public enum TypePayQD
        {
            DUNG_HAN = 1,
            NO_CU = 2,
            TRE_HAN = 3,
            DONG_DU = 4,
            TONG = 5,
        }

        public enum TypeBlockEntity
        {
            BLOCK_NORMAL = 1,
            BLOCK_RENT = 2
        }

        //Hiện trạng sử dụng
        public enum UsageStatus
        {
            DANG_CHO_THUE = 1,
            NHA_TRONG = 2,
            TRANH_CHAP = 3,
            CAC_TRUONG_HOP_KHAC = 4,
        }

        public enum TypeApartmentEntity
        {
            APARTMENT_NORMAL = 1,
            APARTMENT_RENT = 2
        }

        public enum CodeStatus
        {
            CHUA_TON_TAI = 1,
            DA_TON_TAI = 2,
            DA_CAP_NHAT = 3
        }

        public enum TypeUrban
        {
            LOAI_1 = 1,
            LOAI_5 = 5
        }

        public enum TypeAlley          //Loại hẻm 
        {
            MAIN = 1,   //Đất nằm mặt tiền hẻm chính
            EXTRA = 2   //Đất nằm ở hẻm phụ
        }

        public enum TypeApartmentDetail
        {
            BLOCK = 1,
            APARTMENT = 2
        }

        public enum TypeApartmentLandDetail
        {
            BLOCK = 1,
            APARTMENT = 2
        }
        public enum TypeTable
        {
            HSTT = 1, //Bảng hệ số tỉnh thành
            HSTC = 2,//Bảng hệ tầng cao
            K5 = 3,//Bảng hệ số K5
            HSGG = 4,//Bảng hệ số giảm giá
            HSKV = 5,//Bảng hệ số khu vực
            HTKT = 6,//Bảng hệ thống kỹ thuật
            HSGT = 7,//Bảng hệ số giá thuê
        }

        public enum FileStatus
        {
            RECEIVE = 1,//Tiếp nhận
            CHECK = 2,//Kiểm tra
            PROCESSED = 3,//Đã xử lý
        }

        public enum TypeQD
        {
            QD_1753 = 1753, //Quyết định 1753
            QD_09 = 9, //Quyết định 09
            QD_22 = 22,// Quyết định 22-2
        }

        //public enum TypeBlock
        //{
        //    Cap1 = 1,
        //    Cap2 = 2,
        //    Cap3 = 3,
        //    Cap4 = 4,
        //}

        public enum TypeHouse
        {
            Eligible_Sell = 1,//Đủ đkien bán
            Not_Eligible_Sell = 2,//Chưa đủ điều kiện bán
            Not_Sell = 3,//Không đủ điều kiện bán
        }

        //public enum TypeMapCodeIdentify
        //{
        //    BLOCK = 1,
        //    APARTMENT = 2
        //}

        //public enum TypeCodeIdentify
        //{
        //    SELL = 1,   //bán
        //    RENT = 2    //thuê
        //}
        public enum TypeReception
        {
            Received = 1, // đã tiếp nhận
            ReceivedYet = 2, // chưa tiếp nhận
            NotReceived = 3, // không tiếp nhận
        }

        public enum TypeHandover
        {
            Household = 1, // bàn giao cho hộ dân
            Indemnify = 2, // bàn giao cho ban bồi thường 
        }

        public enum PlotType
        {
            UnknowLand = 3,     //Lô chưa xác định
            LandApartment = 1,  //Lô chung cư
            LandPlatform = 2,   //Lô nền đất
        }

        public enum TypeOverYear
        {
            Installment = 1,//trả góp
            TemporaryResidence = 2, // tạm cư
            Rent = 3, // thuê
            OneSell = 4, // bán 1 lần
        }

        //Loại giá của hợp đồng 167
        public enum TypePriceContract167
        {
            GIA_NIEM_YET = 1,
            DAU_GIA = 2
        }

        //Thơi gian thuê - hợp đồng 167
        public enum RentalPeriodContract167
        {
            TAM_BO_TRI = 1,
            THUE_1_NAM = 2,
            THUE_5_NAM = 3,
            KHAC = 4
        }

        //Mục đích thuê - hợp đồng 167
        public enum RentalPurposeContract167
        {
            KINH_DOANH_DV = 1,
            CO_SO_SX = 2,
            KHO_BAI = 3,
            KHAC = 4
        }

        //Kỳ thanh toán - hợp đồng 167
        public enum PaymentPeriodContract167
        {
            THANG = 1,
            QUY = 2
        }

        //Trạng thái hợp đồng 167
        public enum ContractStatus167
        {
            CON_HIEU_LUC = 1,
            HET_HIEU_LUC = 2
        }

        //Loại hợp đồng 167 
        public enum Contract167Type
        {
            MAIN = 1,   //hợp đồng chính
            EXTRA = 2   //phụ lục hợp đồng
        }

        public enum Md167DebtTypeRow
        {
            DONG_THE_CHAN = 1,
            DONG_NAM = 2,
            DONG_DU_LIEU = 3,
            DONG_NO_CU = 4,
            DONG_TONG = 5
        }

        public enum ContractTDCType
        {
            MAIN = 1,   //hợp đồng chính
            EXTRA = 2   //phụ lục hợp đồng
        }
        public enum Land_type
        {
            DAT_O = 1,
            DAT_TMDV = 2,
            DAT_SXKD = 3,
            DAT_PUBLIC_KINHDOANH = 4,
            DAT_COQUAN_CONGTRINH = 5,
        }
        public enum Location
        {
            VI_TRI_1 = 1,
            VI_TRI_2 = 2,
            VI_TRI_3 = 3,
            VI_TRI_4 = 4,
        }

        public enum ImportHistoryType
        {
            Md167Receipt = 1,           //Import phiếu thu cho hợp đồng nhà ở 167
            Md167Landprice = 2,         //Import Bảng giá đất cho nhà ở 167
            Md167House = 3,             //Import Thông tin cơ sở nhà đất
            Md167Kios = 4,              //Import Kios cho nhà ở 167
            Md167MainContract = 5,      //Import danh sách hợp đồng
            Md167ExtraContract = 6,     //Import danh sách phụ lục hợp đồng
            NocSalary = 7,              //Import Hệ số lương
            NocCoefficient = 8,         //Import hệ số thời điêm bố trí
            NocConversion = 9,          //Import hệ số quy đổi
            NocDefaultCoeficient = 10,  //Import hệ số mặc định
            NocDiscountCoefficient = 11,//Import hệ số vị trí
            NocRentingPrice = 12,       //Import bảng giá thuê
            NocBlock = 13,              //Import Căn nhà
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
            Noc_PriceList = 43,                             //Bảng giá nhà ở
            Noc_GeneralDataImport = 44,                     //Import dữ liệu nhà ở cũ từ file excel tổng hợp
            Noc_Landprice = 45,                             //Bảng giá đất nhà ở cũ
            Noc_Contract_Rent_Receipt = 46,                 //Phiếu thu - Hợp đồng thuê NOC
            Noc_Contract_Rent = 47,                         //Hợp đồng thuê NOC
            Common_Department = 70,                             //Phòng ban - danh mục chung
            Common_Position = 71,                               //Chức danh - danh mục chung
            Common_Lane = 72,                                   //Đường - danh mục chung
            NocRentBlock = 14,      //Import căn nhà thuê
            TdcIngredientsPrice = 73,    //Import thành phần bán giá cấu thành - Tdc
            TdcOriginalPriceAndTax = 74, //Import thành phần giá gốc- thuế phí
            TdcProfitValue = 75,         //Import hệ số lãi phạt thuê
            TdcInstallmentRate = 76,     //Import lãi suất trả góp hàng năm
            TdcResettlement = 77,        //Import chung cư
            TdcLand = 78,                //Import lô 
            TdcBlockHouse = 79,          //Import khối nhà tdc
            TdcFloor = 80,               //Import tầng tdc 
            TdcApartment = 81,           //Import căn tdc
            TdcPlatform = 82,            //Import nển tdc
            TdcProject = 83,             //Import dự án tdc
            TdcCustomer = 84,            //Import khách hàng tdc
            Tdc_ApartmentDataImport = 85,//Import danh sách căn hộ trống tiếp nhận và quản lý
            Tdc_PlatformDataImport = 86, //Import danh sách nền đất trống tiếp nhận và quản lý
        }

        public enum TypeDevice //loại device login
        {
            WEB = 1,
            IOS = 2,
            ANDROID = 3
        }

        public enum TypeSupportReq      // Loại yêu cầu hỗ trợ
        {
            RECEIVED = 1, //Đã tiếp nhận
            PROCESSING = 2, //Đang xử lý
            COMPLETE = 3, //hoàn thành
        }

        public enum TypePersonSupport // người tiếp nhận yêu cầu hỗ trợ
        {
            PERSIONNEL= 1, //nhân viên
            MANAGE = 2, //quản lý
        }

        public enum TypeUsageStatus // trạng thái sử dụng
        {
            Eligible =1, // đủ đk 
            Unqualified=2,// chưa đủ đk
            Unconditional=3,// ko đủ đk
        }

        public enum TypeNotificationForm // biểu mẫu thông báo
        {
            Bm001 = 1,
            Bm002 = 2,
            Bm003 = 3,
            Bm004 = 4,
            Bm005 = 5,
        }

        public enum ModuleSystem
        {
            NOC = 1,
            NTDC = 2,
            N167 = 3
        }

        public enum TypeEditHistory
        {
            BLOCK = 1,
            APARTMENT = 2,
            RENT_CONTRACT = 3,
            SELL_PRICING = 4
        }

        public enum TypeSignalRNotify
        {
            LOG_OUT = 1,
            NOTIFY = 2,
            RE_LOGIN = 3
        }
    }
}
