using System;
using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Block : AbstractEntity<int>
    {
        public TypeReportApply TypeReportApply { get; set; }
        public int TypeBlockId { get; set; }
        public int FloorApplyPriceChange { get; set; }
        public string FloorBlockMap { get; set; }
        public string LandNo { get; set; }
        public string MapNo { get; set; }
        public string Width { get; set; }
        public string Deep { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int? Lane { get; set; }
        public int Ward { get; set; }
        public int District { get; set; }
        public int Province { get; set; }
        public byte TypePile { get; set; }
        public string ConstructionAreaNote { get; set; }
        public float ConstructionAreaValue { get; set; }
        public float? ConstructionAreaValue1 { get; set; }
        public float? ConstructionAreaValue2 { get; set; }
        public float? ConstructionAreaValue3 { get; set; }
        public string UseAreaNote { get; set; }
        public float UseAreaValue { get; set; }
        public float? UseAreaValue1 { get; set; }
        public float? UseAreaValue2 { get; set; }
        public int DecreeType1Id { get; set; }      //Nghị định
        public bool? SpecialCase { get; set; }      //Trường hợp nhà tư nhân nhưng có tầng lửng do nhà nước quản lý
        public string LandUsePlanningInfo { get; set; }     //Thông tin quy hoạch sử dụng đất
        public string HighwayPlanningInfo { get; set; }     //Thông tin quy hoạch lộ giới
        public string LandAcquisitionSituationInfo { get; set; }        // Thông tin Tình hình thu hồi đất
        public int? LandPriceItemId_99 { get; set; }
        public double? LandPriceItemValue_99 { get; set; }
        public int? LandPriceItemId_34 { get; set; }
        public double? LandPriceItemValue_34 { get; set; }
        public int? LandPriceItemId_61 { get; set; }
        public double? LandPriceItemValue_61 { get; set; }
        public int? PositionCoefficientId_99 { get; set; }
        public string PositionCoefficientStr_99 { get; set; }
        public LocationResidentialLand? LandscapeLocation_99 { get; set; }
        public float? LandPriceRefinement_99 { get; set; }
        public decimal? LandScapePrice_99 { get; set; }
        public int? PositionCoefficientId_34 { get; set; }
        public string PositionCoefficientStr_34 { get; set; }
        public LocationResidentialLand? LandscapeLocation_34 { get; set; }
        public float? LandPriceRefinement_34 { get; set; }
        public decimal? LandScapePrice_34 { get; set; }
        public int? PositionCoefficientId_61 { get; set; }
        public string PositionCoefficientStr_61 { get; set; }
        public LocationResidentialLand? LandscapeLocation_61 { get; set; }
        public float? LandPriceRefinement_61 { get; set; }
        public decimal? LandScapePrice_61 { get; set; }
        public LevelAlley? LevelAlley_34 { get; set; }
        public LocationResidentialLand? LandscapeLocationInAlley_34 { get; set; }
        public bool? IsAlley_34 { get; set; }
        public int? AlleyPositionCoefficientId_34 { get; set; }
        public string AlleyPositionCoefficientStr_34 { get; set; }
        public decimal? AlleyLandScapePrice_34 { get; set; }
        public string TextBasedInfo { get; set; }       // Căn cứ văn bản số
        public bool? LandSpecial { get; set; }       //Khu đất, thửa đất có hình dáng đặc biệt
        public bool? LandAreaSpecial { get; set; }       //Có phần diện tích không mặt tiền đường (hẻm) từ 15m2 trở lên
        public float? LandAreaSpecialS1 { get; set; }       //S1
        public float? LandAreaSpecialS2 { get; set; }       //S2
        public float? LandAreaSpecialS3 { get; set; }       //S3
        public bool? WidthSpecial { get; set; }       //Phần diện tích có chiều dài lớn gấp nhiều lần so với chiều rộng
        public float? WidthSpecialS1 { get; set; }       //S1A
        public float? WidthSpecialS2 { get; set; }       //S2B
        public float? WidthSpecialS3 { get; set; }       //S3C
        public TypeLandSpecial? LandPositionSpecial { get; set; }       //Vị trí đặc biệt của khu đất
        public TypeCaseApply_34? CaseApply_34 { get; set; }
        public TypeBlockEntity TypeBlockEntity { get; set; }
        //Phần thông tin bổ sung của căn nhà thuê
        public float? CampusArea { get; set; }              //Diện tích khuôn viên
        public bool? EstablishStateOwnership { get; set; }  //Xác lập sở hữu nhà nước
        public bool? Dispute { get; set; }                  //Tranh chấp
        public bool? Blueprint { get; set; }                //Bản vẽ
        public int? CustomerId { get; set; }                //Chủ hộ
        public UsageStatus? UsageStatus { get; set; }       //Hiện trạng sử dụng
        public string UsageStatusNote { get; set; }         //Ghi chú hiện trạng sử dụng
        public string UseAreaNote1 { get; set; }
        public TypeAlley? TypeAlley_61 { get; set; }
        public decimal? No2LandScapePrice_61 { get; set; }
        public bool? IsFrontOfLine_61 { get; set; }
        public LocationResidentialLand? LandscapeLocationInAlley_61 { get; set; }
        public bool? IsAlley_61 { get; set; }
        //Thêm trường cho trường hợp nhà riêng lẻ
        public float? LandscapeAreaValue { get; set; }      //Diện tích đất ở
        public float? LandscapePrivateAreaValue { get; set; }     //Diện tích đất sử dụng riêng
        //Thêm trường cho trường hợp nhà chung cư
        public bool? ApprovedForConstructionOnTheApartmentYard { get; set; }        //TH được công nhận xây dựng trên phần sân chung cư
        //Thêm trường cho trường hợp là Bán phần diện tích sử dụng chung
        public int? ParentId { get; set; }
        public TypeReportApply? ParentTypeReportApply { get; set; }
        public string SellConstructionAreaValue { get; set; }               //Tổng diện tích xây dựng của căn nhà đã bán
        public float? SellConstructionAreaNote { get; set; }                 //Ghi chú tổng diện tích xây dựng đã bán
        public int Floor { get; set; } //Tầng
        public DateTime DateRecord { get; set; } //Ngày ghi nhận
        public bool? TakeOver { get; set; } //Chiếm dụng
        public TypeHouse TypeHouse { get; set; } //Điều kiện hộ
        public DateTime DateApply { get; set; } //Ngày áp dụng điều kiện
        public string Attactment { get; set; }  //File xác minh đính kèm
        public int? UserIdCreateAttactment { get; set; }
        public string No { get; set; }          //Số thứ tự
        public string CodeEstablishStateOwnership { get; set; }  //Số quyết định - Xác lập sở hữu nhà nước
        public DateTime? DateEstablishStateOwnership { get; set; }  //Ngày quyết định - Xác lập sở hữu nhà nước
        public string NameBlueprint { get; set; }                //Tên Bản vẽ
        public DateTime? DateBlueprint { get; set; }                // Ngày lập Bản vẽ
        public bool? ExceedingLimitDeep { get; set; }               //Vượt hạn mức
    }
}
