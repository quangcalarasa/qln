using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class DistrictAllocasionPlatform : AbstractEntity<int>
    {
        public int TdcPlatformManagerId { get; set; }
        public int DistrictId { get; set; }
        public int ActualNumber { get; set; }//số lượng thực tế
    }
}
