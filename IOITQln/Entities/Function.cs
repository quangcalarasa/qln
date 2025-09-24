using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Function : AbstractEntity<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int FunctionParentId { get; set; }
        public string Url { get; set; }
        public string Note { get; set; }
        public int? Location { get; set; }
        public string Icon { get; set; }
        public bool? IsSpecialFunc { get; set; }
        public SubSystem SubSystem { get; set; }
    }
}
