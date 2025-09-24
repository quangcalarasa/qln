using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Md167Valuation : AbstractEntity<long>
    {
        public int Md167ContractId { get; set; }
        public string UnitValuation { get; set; }
        public string Attactment { get; set; }
        public DateTime DateEffect { get; set; }
    }
}
