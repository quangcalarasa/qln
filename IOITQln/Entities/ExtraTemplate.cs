using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraTemplate : AbstractEntity<int>
    {
        public string Name { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
    }
}
