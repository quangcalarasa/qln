using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    //Thuế suất đất
    public class Md167LandTax : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string TypeArea { get; set; }
        public decimal Tax { get; set; }
        public bool IsDefault { get; set; }
        public TypeQD DecisionId { get; set; }
    }
}
