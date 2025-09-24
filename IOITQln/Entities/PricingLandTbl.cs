using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class PricingLandTbl : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public int ApartmentId { get; set; }
        public DecreeEnum? DecreeType1Id { get; set; }
        public TermApply? TermApply { get; set; }
        public int? Level { get; set; }
        public int FloorApplyCoefficient { get; set; }
        public int FloorId { get; set; }
        public int AreaId { get; set; }
        public float? GeneralArea { get; set; }
        public float? PrivateArea { get; set; }
        public float? CoefficientDistribution { get; set; }     //Hệ số tầng
        public float? CoefficientUseValue { get; set; }     //Hệ số điều chỉnh giá trị sử dụng
        public float? MaintextureRateValue { get; set; }
        public int? PriceListItemId { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceInYear { get; set; }
        public decimal? RemainingPrice { get; set; }
        //Thêm trường cho TH căn hộ chung cư
        public int? FloorApplyPriceChange { get; set; }         //Số tầng chung cư để áp dụng hs phân bổ đất
        public bool? ApplyInvestmentRate { get; set; }          //Áp dụng suất vốn đầu tư
        public bool? IsMezzanine { get; set; }
        public int? InvestmentRateItemId { get; set; }          //Chi tiết suất vốn đầu tư áp dụng
        public decimal? InvestmentRateValue { get; set; }       //Suất vốn đầu tư
        public decimal? InvestmentRateValue1 { get; set; }      //Chi phí xây dựng
        public decimal? InvestmentRateValue2 { get; set; }      //Chi phí thiết bị
    }
}
