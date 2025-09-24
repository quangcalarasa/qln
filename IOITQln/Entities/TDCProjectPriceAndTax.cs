using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TDCProjectPriceAndTax : AbstractEntity<int>
    {
        public int TDCProjectId { get; set; }
        public int PriceAndTaxId { get; set; }
        public double Value { get; set; }
        public int Location { get; set; }
    }
}
