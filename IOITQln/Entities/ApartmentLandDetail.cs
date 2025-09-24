using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ApartmentLandDetail : AbstractEntity<long>         //Bảng chi tiết diện tích đất sử dụng của căn hộ theo Nghị định, điều
    {
        public int TargetId { get; set; }
        public DecreeEnum? DecreeType1Id { get; set; }
        public TermApply? TermApply { get; set; }
        public int? Level { get; set; }
        public int FloorId { get; set; }
        public int AreaId { get; set; }
        public float? GeneralArea { get; set; }
        public float? PrivateArea { get; set; }
        public TypeApartmentLandDetail Type { get; set; }
        //Thêm trường cho TH căn hộ chung cư
        public int? FloorApplyPriceChange { get; set; }         //Số tầng chung cư để áp dụng hs phân bổ đất
        public float? CoefficientDistribution { get; set; }     //Hệ số phân bổ tầng
    }
}
