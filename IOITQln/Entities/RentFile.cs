using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class RentFile : AbstractEntity<Guid>
    {
        public int Month { get; set; } //Số tháng (với hợp đồng là 60,còn phụ lục tự diền < 60)
        public byte Type { get; set; } //1 là hợp đồng, 2 là phụ lục 
        public int FileStatus { get; set; } //Loại biên bản áp dụng
        public string Code { get; set; } // Số hợp đồng
        public string CodeHS { get; set; } //Số hồ sơ
        public DateTime DateHD { get; set; } //Ngày kí hợp đồng
        public int CustomerId { get; set; }
        public int RentApartmentId { get; set; }
        public int RentBlockId { get; set; }

        public DateTime? Dob { get; set; } // Ngày tháng năm sinh
        public string AddressKH { get; set; } //Hộ khẩu thường chú
        public string CodeKH { get; set; } //CCCD
        public string Phone { get; set; } // Số điện thoại

        public TypeReportApply TypeReportApply { get; set; } //Loại biên bản áp dụng
        public int TypeBlockId { get; set; } //loại nhà
        public float? CampusArea { get; set; }//Diện tích khuôn viên
        public string fullAddressCN { get; set; } //Dc căn nhà
        public float ConstructionAreaValue { get; set; } //Tổng diện tích sàn xây dựng
        public float UseAreaValueCN { get; set; } //Tổng dt sử dụng
        public string CodeCN { get; set; } //Mã định danh căn nhà
        public string CodeCH { get; set; } //Mã định danh căn hộ
        public float UseAreaValueCH { get; set; } //Tổng dt sử dụng căn hộ
        public string fullAddressCH { get; set; } //Dc căn hộ
        public int DistrictId { get; set; } //ID quận
        public UsageStatus? UsageStatus { get; set; } //Tình trạng nhà
        public TypeHouse TypeHouse { get; set; } //Điều kiện hộ
        public string ProcessProfileCeCode { get; set; } //Ket noi CE
        public int? ParentId { get; set; }
        public int WardId { get; set; }
        public int LaneId { get; set; }
        public int proviceId { get; set; }
        public string CustomerName { get; set; }  //Tên KH
    }
}
