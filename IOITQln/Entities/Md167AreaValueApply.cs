using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167AreaValueApply : AbstractEntity<int>
    {
        public int AreaValueId { get; set; }
        public int DistrictId { get; set; }
    }
}
