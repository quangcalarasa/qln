using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PricingCustomer : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public int CustomerId { get; set; }
    }
}
