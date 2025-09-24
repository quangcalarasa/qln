using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class RatioMainTexture : AbstractEntity<int>
    {
        public int? ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public float? TypeMainTexTure1 { get; set; }
        public float? TypeMainTexTure2 { get; set; }
        public float? TypeMainTexTure3 { get; set; }
        public float? TypeMainTexTure4 { get; set; }
        public float? TypeMainTexTure5 { get; set; }
        public float? TypeMainTexTure6 { get; set; }
    }
}
