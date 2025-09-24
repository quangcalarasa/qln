using IOITQln.Common.Bases;
using IOITQln.Models.Data;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class InstallmentPriceTableMetaTdc : AbstractEntity<int>
    {
        public int TdcIntallmentPriceId { get; set; }
        public int DataRow { get; set; }
        public int DataStatus { get; set; }
        public int? PayTimeId { get; set; }
        public decimal? Pay { get; set; }
        public decimal? Paid { get; set; }
        public decimal? PriceDifference { get; set; }
    }
}
