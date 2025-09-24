using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class NocDataImport
    {
    }

    public class DecreeType2DataImport : Decree
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class CtMaintextureDataImport : CurrentStateMainTexture
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class RatioMaintextureDataImport : RatioMainTexture
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ConstructionPriceDataImport : ConstructionPrice
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class SalaryCoefficientDataImport : SalaryCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class DeductionCoefficientDataImport : DeductionCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class AreaCorrectionCoefficientDataImport : AreaCorrectionCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class No2LandPriceDataImport : No2LandPrice
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class LandSpecialCoefficientDataImport : LandSpecialCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class PositionCoefficientDataImport : PositionCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class LandPriceCorrectionCoefficientDataImport : LandPriceCorrectionCoefficient
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class DeductionLandMoneyDataImport : DeductionLandMoney
    {
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class NocCustomerDataImport : Customer
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class PriceListDataImport : PriceListItem
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string DecreeType2Name { get; set; }
        public string Des { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class PriceListGroupDataImport
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int DecreeType2Id { get; set; }      //Thông tư, văn bản
        public List<PriceListDataImport> dataList { get; set; }
    }

    public class GeneralDataImport
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public bool? ExistInRent { get; set; }
        public long? ExistInRentId { get; set; }
        public bool? ExistInNormal { get; set; }
        public long? ExistInNormalId { get; set; }
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
        public string LoaiBienBan { get; set; }
        public TypeReportApply? LoaiBienBanApDung { get; set; }
        public string SoThuTuCan { get; set; }
        public string SoThuTuHo { get; set; }
        public string MaDinhDanh { get; set; }
        public string SoNha { get; set; }
        public string Duong { get; set; }
        public int? DuongApDung { get; set; }
        public string Phuong { get; set; }
        public int? PhuongApDung { get; set; }
        public string Quan { get; set; }
        public int? QuanApDung { get; set; }
        public int? TinhApDung { get; set; }
        public string LoaiNha { get; set; }
        public int? LoaiNhaApDung { get; set; }
        public string CapNha { get; set; }
        public float? DienTichXayDung { get; set; }
        public float? DienTichKhuonVien { get; set; }
        public float? DienTichDatRieng { get; set; }
        public float? DienTichDatChung { get; set; }
        public float? DienTichDatChungPhanBo { get; set; }
        public float? DienTichSuDungRieng { get; set; }
        public float? DienTichSuDungChung { get; set; }
        public float? DienTichSuDungChungPhanBo { get; set; }
        public float? TongDienTichSuDungChung { get; set; }
        public float? TongDienTichSuDungRieng { get; set; }
        public string NguoiDaiDien { get; set; }
        public string CanCuoc { get; set; }
        public bool? ExistCanCuoc { get; set; }
        public int? ExistCanCuocId { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NoiCap { get; set; }
        public string DiaChiThuongTru { get; set; }
        public string SoDienThoai { get; set; }
        public string ThanhVien { get; set; }
        public string SoQuyetDinh { get; set; }
        public DateTime? NgayQuyetDinh { get; set; }
        public string TenBanVe { get; set; }
        public DateTime? NgayLapBanVe { get; set; }
        public string TinhTrangNha { get; set; }
        public UsageStatus? TinhTrangNhaApDung { get; set; }
        public string GhiChu { get; set; }
    }

    public class ResGeneralDataImport
    {
        public List<GeneralDataImport> data { get; set; }
        public List<Block> blocks { get; set; }
        public List<Apartment> apartments { get; set; }
    }

    public class SimpleData
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SimpleLaneData
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Ward { get; set; }
    }

    public class SimpleBlockData : SimpleData
    {
        public TypeBlockEntity TypeBlockEntity { get; set; }
        public TypeReportApply TypeReportEntity { get; set; }
    }

    public class SimpleApartmentData : SimpleData
    {
        public TypeApartmentEntity TypeApartmentEntity { get; set; }
        //public TypeReportApply TypeReportEntity { get; set; }
    }

    public class GeneralDataImportCount
    {
        public int TotalBlockRent { get; set; }
        public int TotalBlockNormal { get; set; }
        public int TotalApartmentRent { get; set; }
        public int TotalApartmentNormal { get; set; }

        public GeneralDataImportCount(int totalBlockRent, int totalBlockNormal, int totalApartmentRent, int totalApartmentNormal)
        {
            TotalBlockRent = totalBlockRent;
            TotalBlockNormal = totalBlockNormal;
            TotalApartmentRent = totalApartmentRent;
            TotalApartmentNormal = totalApartmentNormal;
        }
    }

    public class PromissoryDataImport : NocReceipt
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class ContractRentDataImport
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
        public  string NoContract { get; set; }     //Số hợp đồng
        public string CodeContract { get; set; }    //Mã định danh hợp đồng
        public DateTime? DateSign { get; set; }
        public string ContractStatus { get; set; }
        public string CustomerName { get; set; }
        public string IdentityCode { get; set; }
        public DateTime? Dob { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string TypeReportApplyStr { get; set; }
        public string Code { get; set; }            //Mã căn nhà/ căn hộ
    }

    public class MonthYearPromissory
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public bool? IsFullYear { get; set; }
    }

    public class ContractRentDataImportType2
    {
        public int? Index { get; set; }//số thứ tự căn 
        public string? CodeHD { get; set; } //Số hợp đồng
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
        public string CustomerName { get; set; } //Tên khách hàng
        public string Address { get; set; } //Số nhà
        public string Lane { get; set; } //Đường
        public int LaneId { get; set; }
        public string Distric { get; set; } //Quận
        public int DistricId { get; set; }
        public int ProviceId { get; set; } //Tỉnh/thành phố
        public string Provice { get; set; } //Tỉnh/thành phố
        public string Ward { get; set; } //Phường
        public int WardId { get; set; } 
        public string Code { get; set; }  //Mã định danh 
        public string FullAddress { get; set; }  //Địa chỉ đầy dủ
        public string Note { get; set; } //ghi chú
        public decimal? StillDbest { get; set; } //Còn nợ
        public decimal? PriceRent { get; set; } //Giá thuê/tháng
        public DateTime DateAssign { get; set;} //Ngày ký hợp đồng
        public string ConfirmationNumber { get; set; }  //Số xác nhận
        public string CCCD { get; set; } //Căn cước công dân
        public DateTime? PlacementTime { get; set; } //Thời điểm bố trí
        public string? Note2 { get; set; } //Sai MDD
        public string? SXN { get; set; } //Số xác nhận
    }

    public class WrongDataUpdate
    {
        public string Address { get; set; }
        public int? Lane { get; set; }
        public Apartment apartment { get; set; }
    }

    public class WrongDataFromTemplate
    {
        public string LoaiBienBan { get; set; }
        public TypeReportApply? LoaiBienBanApDung { get; set; }
        public string MaDinhDanh { get; set; }
        public string DiaChiNha { get; set; }
        public string DiaChiCanHo { get; set; }
    }

    public class WrongCustomerData
    {
        public string LoaiBienBan { get; set; }
        public TypeReportApply? LoaiBienBanApDung { get; set; }
        public string MaDinhDanh { get; set; }
        public string NguoiDaiDien { get; set; }
        public string CanCuoc { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NoiCap { get; set; }
        public string HoKhauThuongTru { get; set; }
        public string DiaChiLienHe { get; set; }
        public string SoDienThoai { get; set; }
    }
}
