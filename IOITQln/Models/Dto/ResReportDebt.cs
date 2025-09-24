using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Dto
{
    public class ResReportDebt
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int Count { get; set; }
        public List<ResReportDebtItem> resReportDebtItems { get; set; }
    }

    public class ResReportDebtItem
    {
        public int? HouseId { get; set; }
        public string HouseName { get; set; }
        public decimal? AmountToBePaid { get; set; }         //Tổng số phải thu
        public decimal? AmountPaid { get; set; }            //Tổng số đã thu
        public decimal? AmountDiff { get; set; }            //Tổng số còn nợ
        public decimal? AmountUsed { get; set; }            //Tổng số tiền đã sử dụng
        public decimal? AmountSubmitted { get; set; }       //Tổng số đã nộp
        public decimal? AmountRemaining { get; set; }       //Còn lại
    }
}
