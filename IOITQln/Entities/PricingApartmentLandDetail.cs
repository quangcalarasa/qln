using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class PricingApartmentLandDetail : AbstractEntity<long>         //Bảng chi tiết tính diện tích chung quy đổi, tiền đất theo từng điều và nghị định
    {
        public int PricingId { get; set; }
        public long ApartmentLandDetailId { get; set; }
        public int ApartmentId { get; set; }
        public DecreeEnum? DecreeType1Id { get; set; }
        public TermApply? TermApply { get; set; }
        public float? GeneralArea { get; set; }
        public float? PrivateArea { get; set; }
        public decimal? LandUnitPrice { get; set; }                 //Đơn giá đất
        public decimal? LandPrice { get; set; }                 //Giá đất
        public int? DeductionLandMoneyId { get; set; }
        public float? DeductionLandMoneyValue { get; set; }
        public float? ConversionArea { get; set; }
        public decimal? LandPriceAfterReduced { get; set; }
        //Thêm trường cho TH nhà riêng lẻ
        public float? InLimitArea { get; set; }             //Diện tích đất trong hạn mức
        public float? InLimitPercent { get; set; }          //Hạn mức áp dụng vs Diện tích đất trong hạn mức
        public float? OutLimitArea { get; set; }            //Diện tích đất ngoài hạn mức
        public float? OutLimitPercent { get; set; }            //Hạn mức áp dụng vs Diện tích đất trong hạn mức
        public float? SumLimitArea { get; set; }            //Diện tích cộng dồn
        public string SumLimitAreaStr { get; set; }            //Diện tích cộng dồn kiểu chuỗi
        public float? LandscapeAreaLimit { get; set; }    //Hạn mức đất ở
        public int? LandscapeLimitItemId { get; set; }
        //Thêm trường cho TH căn hộ chung cư
        public int? FloorApplyPriceChange { get; set; }         //Số tầng chung cư để áp dụng hs phân bổ đất
        public float? CoefficientDistribution { get; set; }     //Hệ số phân bổ tầng
    }
}
