using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class LandscapeLimitItem : AbstractEntity<int>           //Hạn mức đất ở
    {
        public int LanscapeLimitId { get; set; }
        public int? DistrictId { get; set; }
        public float? LimitAreaNormal { get; set; }
        public float? LimitAreaSpecial { get; set; }
        public float? InLimitPercent { get; set; }
        public float? OutLimitPercent { get; set; }

    }
}
