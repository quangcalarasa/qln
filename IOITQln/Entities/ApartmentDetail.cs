using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ApartmentDetail : AbstractEntity<long>
    {
        public DecreeEnum? DecreeType1Id { get; set; }
        public TermApply? TermApply { get; set; }
        public int TargetId { get; set; }
        public int? Level { get; set; }
        //public int? FloorApplyCoefficient { get; set; }
        public int FloorId { get; set; }
        public int AreaId { get; set; }
        public float? TotalAreaFloor { get; set; }
        public float? TotalAreaDetailFloor { get; set; }
        public float? GeneralArea { get; set; }
        public float? PrivateArea { get; set; }
        public float? YardArea { get; set; }                    //Diện tích sân chung phân bổ
        public float? CoefficientDistribution { get; set; }     //Hệ số phân bổ tầng
        public float? CoefficientUseValue { get; set; }         //Hệ số điều chỉnh giá trị sử dụng
        public TypeApartmentDetail Type { get; set; }
        //Thêm trường cho TH căn hộ chung cư
        public int? FloorApplyPriceChange { get; set; }         //Số tầng chung cư để áp dụng hs phân bổ đất
        public bool? ApplyInvestmentRate { get; set; }          //Áp dụng suất vốn đầu tư
        public bool? IsMezzanine { get; set; }
        public DateTime? DisposeTime { get; set; }              //Thời điểm bố trí
    }
}
