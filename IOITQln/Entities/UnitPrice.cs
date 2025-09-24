using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class UnitPrice : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
