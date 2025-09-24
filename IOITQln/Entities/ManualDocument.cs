using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ManualDocument : AbstractEntity<int>
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public string Attactment { get; set; }
        public ModuleSystem Type { get; set; }
    }
}
