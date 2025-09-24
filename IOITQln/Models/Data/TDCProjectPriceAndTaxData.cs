using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TDCProjectPriceAndTaxData : TDCProjectPriceAndTax
    {
        public string PATName { get; set; }
        public List<TDCProjectPriceAndTaxDetailData> TDCProjectPriceAndTaxDetails { get; set; }   
    }
}
