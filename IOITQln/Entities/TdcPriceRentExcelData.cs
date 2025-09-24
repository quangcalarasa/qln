using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRentExcelData : AbstractEntity<int>
    {
        public int? CountYear { get; set; }
        public int? index { get; set; }
        public int? TdcPriceRentExcelMetaId { get; set; }
        public string? PaymentTimes { get; set; } // Lần Trả
        public DateTime? PaymentDatePrescribed { get; set; } //Ngày Thanh Toán Theo QĐ
        public DateTime? PaymentDatePrescribed1 { get; set; } //Ngày Thanh toán Dự Kiến
        public DateTime? ExpectedPaymentDate { get; set; } // Ngày Thanh Toán Thực Tế
        public int? DailyInterest { get; set; }//Thời gian tính lãi theo ngày
        public decimal? DailyInterestRate { get; set; } //Lãi suất tính theo ngày
        public decimal? UnitPay { get; set; }//Số tiền phải trả từng tháng 
        public decimal? PriceEarnings { get; set; } //Số tiền lãi phát sinh do chậm thanh toán 
        public decimal? PricePaymentPeriod { get; set; } //Số tiền đến kỳ phải thanh toán
        public decimal? Pay { get; set; } //Số tiền phải thanh toán
        public decimal? Paid { get; set; } //Số tiền đã thanh toán
        public decimal? PriceDifference { get; set; } // Số tiền chênh lệch
        public string? Note { get; set; } // Ghi Chú
    }
}
