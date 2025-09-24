using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class DiscountCoefficient : AbstractEntity<int>
    {
        public double Value { get; set; }
        public DateTime DoApply { get; set; }
        public int UnitPriceId { get; set; }
        public string Note { get; set; }

    }
}
