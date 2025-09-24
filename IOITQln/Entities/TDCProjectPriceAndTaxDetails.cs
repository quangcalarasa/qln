using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TDCProjectPriceAndTaxDetails : AbstractEntity<int>
    {
        public int PriceAndTaxId { get; set; }
        public int IngredientsPriceId { get; set; }
    }
}
