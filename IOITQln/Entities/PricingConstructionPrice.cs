using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PricingConstructionPrice : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public int ConstructionPriceId { get; set; }
        public int? Year { get; set; }
        public float? Value { get; set; }
    }
}
