using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class No2LandPrice : AbstractEntity<int>           //Bảng Giá đất số 2
    {
        public double? StartValue { get; set; }
        public double? EndValue { get; set; }
        public double? MainPriceLess2M { get; set; }
        public double? ExtraPriceLess2M { get; set; }
        public double? MainPriceLess3M { get; set; }
        public double? ExtraPriceLess3M { get; set; }
        public double? MainPriceLess5M { get; set; }
        public double? ExtraPriceLess5M { get; set; }
        public double? MainPriceGreater5M { get; set; }
        public double? ExtraPriceGreater5M { get; set; }
        public string Note { get; set; }
        public TypeUrban? TypeUrban { get; set; }
    }
}
