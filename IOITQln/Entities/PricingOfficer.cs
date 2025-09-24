using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PricingOfficer : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public string Name { get; set; }
        public string Function { get; set; }
    }
}
