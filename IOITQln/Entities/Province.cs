using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Province : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
