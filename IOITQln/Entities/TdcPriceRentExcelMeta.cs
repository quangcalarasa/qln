using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRentExcelMeta : AbstractEntity<int>
    {
        public int TdcPriceRentId { get; set; }
        public int DataRow { get; set; }
        public int DataStatus { get; set; }
        public int? PayTimeId { get; set; }
        public decimal? Pay { get; set; }
        public decimal? Paid { get; set; }
        public decimal? PriceDifference { get; set; }
        public bool Check { get; set; }
    }
}
