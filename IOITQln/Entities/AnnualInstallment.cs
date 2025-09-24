using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class AnnualInstallment : AbstractEntity<int>
    {
        public int UnitPriceId { get; set; }
        public double Value { get; set; }
        public DateTime DoApply { get; set; }
        public string Note { get; set; }
    }
}
