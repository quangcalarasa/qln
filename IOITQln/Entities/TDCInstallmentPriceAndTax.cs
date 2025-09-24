using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TDCInstallmentPriceAndTax : AbstractEntity<int>
    {
        public int TDCInstallmentPriceId { get; set; }
        public int Year { get; set; }
        public decimal Value { get; set; }//số tiền
    }
}
