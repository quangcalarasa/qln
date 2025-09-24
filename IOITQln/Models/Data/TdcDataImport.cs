using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;


namespace IOITQln.Models.Data
{
    public class TdcDataImport
    {
    }

    public class IngredientsPriceDataImport : IngredientsPrice
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class OriginalPriceAndTaxDataImport : OriginalPriceAndTax
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ProfitValueDataImport : ProfitValue
    {
        public string UnitPriceName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class AnnualInstallmentDataImport : AnnualInstallment
    {
        public string UnitPriceName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ResettlementApartmentDataImport : ResettlementApartment
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class LandDataImport : Land
    {
        public string TDCProjectName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class BlockHouseDataImport : BlockHouse
    {
        public string LandName { get; set; }
        public string TDCProjectName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class FloorTdcDataImport : FloorTdc
    {
        public string BlockHouseName { get; set; }
        public string LandName { get; set; }
        public string TDCProjectName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ApartmentTdcImport : ApartmentTdc
    {
        public string FloorTdcName { get; set; }
        public string BlockHouseName { get; set; }
        public string LandName { get; set; }
        public string TDCProjectName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class PlatformTdcImport : PlatformTdc
    {
        public string LandName { get; set; }
        public string TDCProjectName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class TDCProjectDataImport : TDCProject
    {
        public string LaneName { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class TdcCustomerDataImport : TdcCustomer
    {
        public string WardNameTT { get; set; }
        public string DistrictNameTT { get; set; }
        public string ProvinceNameTT { get; set; }
        public string WardNameLH { get; set; }
        public string DistrictNameLH { get; set; }
        public string ProvinceNameLH { get; set; }
        public string LaneTt { get; set; }
        public string LaneLh { get; set; }
        public string TdcProjectName { get; set; }
        public string TdcLandName { get; set; }
        public string TdcBlockHouseName { get; set; }
        public string TdcFloorName { get; set; }
        public string TdcApartmentName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ApartmentDataImport : DistrictAllocasionApartment
    {
        //public string Identifier { get; set; } // mã định danh
        public int? BlockHouseId { get; set; } //mã khối --> group 4
        public int? LandId { get; set; }//mã lô --> group 3
        public int? FloorTdcId { get; set; }//mã tầng --> group 5
        public int? TdcProjectId { get; set; }//mã dự án ->> group 2
        public int? ApartmentTdcId { get; set; }// mã căn --> group 6
        public string ApartmentTdcName { get; set; }//tên căn
        public string TdcProjectName { get; set; }  //Tên dự án
        public string TdcBlockName { get; set; }    //Tên khối
        public string TdcLandName { get; set; }         //Tên lô
        public string TdcFloorName { get; set; }        //Tên tầng 
        public string DistrictName { get; set; }//tên quận/huyện
        public int DistrictProjectId { get; set; }//Mã quận/huyện dự án ->> Số thứ tự group 1
        public string DistrictProjectName { get; set; } // Tên quận/ huyện của dự án 
        public int TypeDecisionId { get; set; }//mã quyết định
        public string TypeDecisionName { get; set; }// tên quyết định
        public int TypeLegalId { get; set; }//mã pháp lý tiếp nhận
        public string TypeLegalName { get; set; }//tên pháp lý
        public int TdcApartmentCountRoom { get; set; }// Sl phòng ngủ
        public double TdcApartmentArea { get; set; }// Diện tích căn hộ
        public int Qantity { get; set; }// Số lượng phân bổ theo quy định
        public int ReceiveNumber { get; set; }// Số lượng tiếp nhận theo quy định
        public DateTime ReceptionDate { get; set; }// Thời gian tiếp nhận
        public TypeReception? ReceptionTime { get; set; }//Chọn tiếp nhận
        public TypeHandover? HandOver { get; set; } // Chọn bàn giao
        public string ReasonReceivedYet { get; set; }// lý do chưa tiếp nhận
        public string Reminded { get; set; }// đã nhắc nhở
        public string ReasonNotReceived { get; set; }//lý do không tiếp nhận
        public string HandOverYear { get; set; }//Năm bàn giao
        public TypeOverYear? OverYear { get; set; }// chọn theo năm bàn giao
        public bool? HandoverPublic { get; set; }//DVCI bàn giao
        public string NoteHandoverPublic { get; set; }//ghi chú theo DVCI bàn giao
        public bool? HandoverCenter { get; set; }//Trung tâm giao
        public string NoteHandoverCenter { get; set; }//ghi chú theo Trung tâm giao
        public bool? HandoverOther { get; set; }// Bàn giao khác
        public string NoteHandoverOther { get; set; }//ghi chú theo bàn giao khác
        public string Note { get; set; }//Ghi chú
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
        
    }

    public class PlatformDataImport: DistrictAllocasionPlatform
    {
        //public string Identifier { get; set; } // mã định danh
        public int? LandId { get; set; }//mã lô
        public int? TdcProjectId { get; set; }//mã dự án
        public int? PlatformTdcId { get; set; }// mã căn
        public string PlatformTdcName { get; set; }//tên nền
        public string TdcProjectName { get; set; }  //Tên dự án
        public string TdcLandName { get; set; }         //Tên lô
        public string DistrictAllocationName { get; set; }//tên quận/huyện phân bổ
        public int DistrictProjectId { get; set; }//Mã quận/huyện dự án
        public string DistrictProjectName { get; set; } // Tên quận/ huyện của dự án 
        public int TypeDecisionId { get; set; }//mã quyết định
        public string TypeDecisionName { get; set; }// tên quyết định
        public int TypeLegalId { get; set; }//mã pháp lý tiếp nhận
        public string TypeLegalName { get; set; }//tên pháp lý
        public double TdcLength { get; set; }// Chiều dài nền đất
        public double TdcWidth { get; set; }//Chiều rộng nền đất
        public decimal TdcPlatformArea { get; set; }// Diện tích nền đất
        public int PlatCount { get; set; }// Số nền đất
        public int Qantity { get; set; }// Số lượng phân bổ theo quy định
        public int ReceiveNumber { get; set; }// Số lượng tiếp nhận theo quy định
        public DateTime ReceptionDate { get; set; }// Thời gian tiếp nhận
        public TypeReception? ReceptionTime { get; set; }//Chọn tiếp nhận
        public TypeHandover? HandOver { get; set; } // Chọn bàn giao
        public string ReasonReceivedYet { get; set; }// lý do chưa tiếp nhận
        public string Reminded { get; set; }// đã nhắc nhở
        public string ReasonNotReceived { get; set; }//lý do không tiếp nhận
        public string HandOverYear { get; set; }//Năm bàn giao
        public bool? HandoverPublic { get; set; }//DVCI bàn giao
        public string NoteHandoverPublic { get; set; }//ghi chú theo DVCI bàn giao
        public bool? HandoverCenter { get; set; }//Trung tâm giao
        public string NoteHandoverCenter { get; set; }//ghi chú theo Trung tâm giao
        public bool? HandoverOther { get; set; }//Bàn giao khác
        public string NoteHandoverOther { get; set; }//ghi chú theo bàn giao khác
        public string Note { get; set; }//Ghi chú
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
