using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TdcPriceOneSellTax : AbstractEntity<int>
    {
        public int TdcPriceOneSellId { get; set; }
        public int Year { get; set; }
        public decimal Total { get; set; }
    }
}
