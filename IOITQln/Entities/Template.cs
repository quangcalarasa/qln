using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Template : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? Type { get; set; }
        public string ParentName { get; set; }
        public string Attactment { get; set; }
    }
}
