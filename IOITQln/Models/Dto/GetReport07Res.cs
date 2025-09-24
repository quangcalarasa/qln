using IOITQln.Entities;
using System;

namespace IOITQln.Models.Dto
{
    public class GetReport07Res
    {
        public int Md167ContractId { get; set; }
        public string CustomerName { get; set; }
        public string HouseNumber { get; set; }
        public int District { get; set; } // Quận.
        public string DistrictName { get; set; } // Quận.
        public int Ward { get; set; } // Xã.
        public string WardName { get; set; } // Xã.
        public string LaneName { get; set; } // Đường.
        public int Lane { get; set; } // Đường.
        public string Code { get; set; }
        public string DateSign { get; set; }
        public string DateGroundHandover { get; set; }
        public string DatePayment { get; set; }
        public string BillCode { get; set; }
        public string BillDate { get; set; }
        public decimal? RentCostContract { get; set; }
        public decimal? Pay { get; set;}
        public decimal? Paid { get; set;}
        public decimal? PriceDiff { get; set;}
        public string Note { get; set; }
        public Md167Contract md167Contract { get; set; }
    }
}
