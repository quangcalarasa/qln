using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TypeAttributeItem : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int TypeAttributeId { get; set; }
        public bool? IsActive { get; set; }
        public int? Location { get; set; }

    }
}
