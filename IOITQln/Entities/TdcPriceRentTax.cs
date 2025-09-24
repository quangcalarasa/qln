using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRentTax: AbstractEntity<int>
    {
        public int TdcPriceRentId { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
    }
}
