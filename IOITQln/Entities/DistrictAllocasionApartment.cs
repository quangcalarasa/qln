using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class DistrictAllocasionApartment: AbstractEntity<int>
    {
        public int TdcApartmentManagerId { get; set; }
        public int DistrictId { get; set; }
        public int ActualNumber { get; set; }//số lượng thực tế
    }
}
