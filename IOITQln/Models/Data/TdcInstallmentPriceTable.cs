using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class TdcInstallmentPriceTable
    {
        public TypePayQD? RowStatus { get; set; }
        public int? PayTimeId { get; set; }
        public int? TypeRow { get; set; }
        public int DataStatus { get; set; }
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
        public bool? publicPay { get; set; }
        public bool IsImport { get; set; }
        public TdcInstallmentPriceTable() {
            IsImport = false;   
        }

        public TdcInstallmentPriceTable(int status, TypePayQD? rowStatus, int? payTimeId, int? paymentTimes, DateTime? payDateDefault, DateTime? payDateBefore, DateTime? payDateGuess, DateTime? payDateReal, int? monthInterest, int? dailyInterest, decimal? monthInterestRate, decimal? dailyInterestRate, decimal? totalPay, decimal? payAnnual, decimal? totalInterest, decimal? totalPayAnnual, decimal? pay, decimal? paid, decimal? priceDifference, string note)
        {
            this.DataStatus = status;
            RowStatus = rowStatus;
            PayTimeId = payTimeId;
            PaymentTimes = paymentTimes;
            PayDateDefault = payDateDefault;
            PayDateBefore = payDateBefore;
            PayDateGuess = payDateGuess;
            PayDateReal = payDateReal;
            MonthInterest = monthInterest;
            DailyInterest = dailyInterest;
            MonthInterestRate = monthInterestRate;
            DailyInterestRate = dailyInterestRate;
            TotalPay = totalPay;
            PayAnnual = payAnnual;
            TotalInterest = totalInterest;
            TotalPayAnnual = totalPayAnnual;
            Pay = pay;
            Paid = paid;
            PriceDifference = priceDifference;
            Note = note;
        }
        public class InstallmentPriceTableMetaTdcData : InstallmentPriceTableMetaTdc
        {
            public List<InstallmentPriceTableTdc> installmentPriceTableTdcs { get; set; }

        }

    }
}
