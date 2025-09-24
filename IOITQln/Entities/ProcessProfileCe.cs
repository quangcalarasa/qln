using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ProcessProfileCe : AbstractEntity<long>
    {
        public string IdServiceRecord { get; set; }
        public string Code { get; set; }
        public string CodeIdentify { get; set; }
    }
}
