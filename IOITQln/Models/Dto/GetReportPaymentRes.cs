using System;

namespace IOITQln.Models.Dto
{
    public class GetReportPaymentRes
    {
        public int Year { get; set; }
        public DateTime Date { get; set; }// ngày nộp hợp đồng
        public string HouseName { get; set; }
        public string HouseCode { get; set; }
        public string DistrictName { get; set; }
        public string WardName { get; set; }
        public string LaneName { get; set; }
        public decimal? TaxNN { get; set; }//thuế đất phí nông nghiệp của nhà đất(Nhà đất và kios dùng)
        public decimal? Paid { get; set; } // số tiền đã đóng
        public decimal? Debt { get; set; } // số tiền phải đóng
    }
}
