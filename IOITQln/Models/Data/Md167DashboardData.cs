using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Data
{
    public class Md167DashboardData
    {
    }

    public class Md167HouseBaseDashboardData
    {
        public int TotalValue { get; set; } = 0;
        public int NotRentValue { get; set; } = 0;
        public int RentValue { get; set; } = 0;
        public int OtherValue { get; set; } = 0;
    }

    public class Md167NewHouseBaseDashboardData
    {
        public string DistrictName { get; set; }
        public int TypeHouse1 { get; set; }
        public int TypeHouse2 { get; set; }
    }

    public class Md167RevenueDashboardData
    {
        public decimal? AmountToBePaid { get; set; } = 0;
        public decimal? AmountPaid { get; set; } = 0;
        public decimal? AmountDiff { get; set; } = 0;
    }

    public class Md167TaxDashboardData
    {
        public int Year { get; set; }
        public decimal? AmountToBePaid { get; set; } = 0;
        public decimal? AmountPaid { get; set; } = 0;
        public decimal? AmountDiff { get; set; } = 0;
    }
}
