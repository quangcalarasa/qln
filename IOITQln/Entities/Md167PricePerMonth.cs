using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Md167PricePerMonth : AbstractEntity<long>
    {
        public int Md167ContractId { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal HousePrice { get; set; }
        public decimal? LandPrice { get; set; }
        public decimal VatPrice { get; set; }
        public DateTime DateEffect { get; set; }
        public float? VatValue { get; set; }
    }
}
