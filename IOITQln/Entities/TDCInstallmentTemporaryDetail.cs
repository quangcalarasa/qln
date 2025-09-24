using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TDCInstallmentTemporaryDetail : AbstractEntity<int>
    {
        public int TDCInstallmentPriceId { get; set; }
        public int IngredientsPriceId { get; set; }
        public decimal Area { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
    }
}
