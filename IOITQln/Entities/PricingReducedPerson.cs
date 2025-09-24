using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PricingReducedPerson : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public int CustomerId { get; set; }
        public float? Year { get; set; }
        public decimal? Salary { get; set; }
        public float? Value { get; set; }
        public float? DeductionCoefficient { get; set; }
    }
}
