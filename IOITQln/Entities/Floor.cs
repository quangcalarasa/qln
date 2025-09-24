using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Floor : AbstractEntity<int>
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
