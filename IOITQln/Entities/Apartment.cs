using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Apartment : AbstractEntity<int>
    {
        public TypeReportApply TypeReportApply { get; set; }
        public int BlockId { get; set; }
        public string Code { get; set; }
        //public string Name { get; set; }
        public string Address { get; set; }
        //public int Lane { get; set; }
        //public int Ward { get; set; }
        //public int District { get; set; }
        //public int Province { get; set; }
        public float? ConstructionAreaValue { get; set; }
        public float? ConstructionAreaValue1 { get; set; }
        public float? ConstructionAreaValue2 { get; set; }
        public float? ConstructionAreaValue3 { get; set; }
        public float? UseAreaValue { get; set; }
        public float? UseAreaValue1 { get; set; }
        public float? UseAreaValue2 { get; set; }
        public float? LandscapeAreaValue { get; set; }
        public float? LandscapeAreaValue1 { get; set; }
        public float? LandscapeAreaValue2 { get; set; }
        public float? LandscapeAreaValue3 { get; set; }
        public bool? InLimit40Percent { get; set; }
        public bool? OutLimit100Percent { get; set; }
        public TypeApartmentEntity TypeApartmentEntity { get; set; }
        //Phần thông tin bổ sung của căn nhà thuê
        public float? CampusArea { get; set; }              //Diện tích khuôn viên
        public bool? EstablishStateOwnership { get; set; }  //Xác lập sở hữu nhà nước
        public bool? Dispute { get; set; }                  //Tranh chấp
        public bool? Blueprint { get; set; }                //Bản vẽ
        public int? CustomerId { get; set; }                //Chủ hộ
        public UsageStatus? UsageStatus { get; set; }       //Hiện trạng sử dụng
        public string UsageStatusNote { get; set; }         //Ghi chú hiện trạng sử dụng
        public string UseAreaNote1 { get; set; }
        //Thêm trường cho trường hợp nhà chung cư
        public bool? ApprovedForConstructionOnTheApartmentYard { get; set; }        //TH được công nhận xây dựng trên phần sân chung cư (DT xây dựng)
        public bool? ApprovedForConstructionOnTheApartmentYardLandscape { get; set; }        //TH được công nhận xây dựng trên phần sân chung cư (DT đất ở)
        public string ConstructionAreaNote { get; set; }    //Ghi chú tổng DT xây dựng
        public string UseAreaNote { get; set; }             //Ghi chú tổng DT sử dụng
        //Thêm trường cho trường hợp là Bán phần diện tích sử dụng chung
        public TypeReportApply? ParentTypeReportApply { get; set; }
        public int Floor { get; set; } //Tầng
        public DateTime DateRecord { get; set; } //Ngày ghi nhận
        public bool? TakeOver { get; set; } //Chiếm dụng
        public TypeHouse TypeHouse { get; set; } //Điều kiện hộ
        public DateTime DateApply { get; set; } //Ngày áp dụng điều kiện
        public string No { get; set; }          //Số thứ tự
        public string CodeEstablishStateOwnership { get; set; }  //Số quyết định - Xác lập sở hữu nhà nước
        public DateTime? DateEstablishStateOwnership { get; set; }  //Ngày quyết định - Xác lập sở hữu nhà nước
        public string NameBlueprint { get; set; }                //Tên Bản vẽ
        public DateTime? DateBlueprint { get; set; }                // Ngày lập Bản vẽ
    }
}
