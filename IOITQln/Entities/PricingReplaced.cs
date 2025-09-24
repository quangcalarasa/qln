using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class PricingReplaced : AbstractEntity<long>
    {
        public int PricingId { get; set; }
        public int PricingReplacedId { get; set; }
        public DateTime? DateCreate { get; set; }
    }
}
