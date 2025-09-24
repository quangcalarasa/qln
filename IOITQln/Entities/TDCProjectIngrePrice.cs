using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TDCProjectIngrePrice : AbstractEntity<int>
    {
        public  int TDCProjectId { get; set; }
        public int IngredientsPriceId { get; set; }
        public double Value { get; set; }
        public int Location  {get; set; }
    }
}
