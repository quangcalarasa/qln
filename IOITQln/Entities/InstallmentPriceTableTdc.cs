using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class InstallmentPriceTableTdc : AbstractEntity<int>
    {
        public int InstallmentPriceTableMetaTdcId { get; set; }
        public int Location { get; set; }
        public TypePayQD? RowStatus { get; set; }
        public int? PayTimeId { get; set; }
        public byte? TypeRow { get; set; }
        public int StatusTdcTable { get; set; }
        public int? PaymentTimes { get; set; } // Lần Trả
        public DateTime? PayDateDefault { get; set; } //Ngày Thanh Toán Theo QĐ
        public DateTime? PayDateBefore { get; set; } // Ngày Thanh Toán Kỳ trước
        public DateTime? PayDateGuess { get; set; } // Ngày Thanh Toán dự kiến
        public DateTime? PayDateReal { get; set; } // Ngày Thanh Toán Thực Tế
        public int? MonthInterest { get; set; }//Thời gian tính lãi theo tháng
        public int? DailyInterest { get; set; }//Thời gian tính lãi theo ngày
        public decimal? MonthInterestRate { get; set; } //Lãi suất tính theo tháng
        public decimal? DailyInterestRate { get; set; } //Lãi suất tính theo ngày
        public decimal? TotalPay { get; set; }//Số tiền gốc tĩnh lãi
        public decimal? PayAnnual { get; set; }//Số tiền gốc phải trả hàng năm
        public decimal? TotalInterest { get; set; }//Lãi phát sinh theo ngày và tháng 
        public decimal? TotalPayAnnual { get; set; } //Số tiền đến kỳ phải thanh toán
        public decimal? Pay { get; set; } //tổng số tiền phải thanh toán
        public decimal? Paid { get; set; } //Số tiền đã thanh toán
        public decimal? PriceDifference { get; set; } // Số tiền chênh lệch
        public string? Note { get; set; }
    }
}
