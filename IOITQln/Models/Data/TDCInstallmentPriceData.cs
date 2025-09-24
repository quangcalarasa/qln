using IOITQln.Entities;
using System;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TDCInstallmentPriceData : TDCInstallmentPrice
    {
        public string CustomerName { get; set; }
        public string TdcProjectName { get; set; }
        public string TdcLandName { get; set; }
        public string TdcBlockHouseName { get; set; }
        public string TdcFloorName { get; set; }
        public string TdcApartmentName { get; set; }
        public List<TDCInstallmentOfficialDetailData> tDCInstallmentOfficialDetails { get; set; }
        public List<TDCInstallmentPriceAndTax> tDCInstallmentPriceAndTaxs { get; set; }
        public List<TDCInstallmentTemporaryDetailData> tDCInstallmentTemporaryDetails { get; set; }

    }
    public class TdcInstallmentPriceGroupByPayTimeId
    {
        public int DataStatus { get; set; }
        public Object? PayTimeId { get; set; }
        public decimal? Pay { get; set; }
        public decimal? Paid { get; set; }
        public decimal? PriceDifference { get; set; }
        public bool? publicPay { get; set; }
        public List<TdcInstallmentPriceTable> tdcInstallmentPriceTables { get; set; }
    }
    public class TdcInstallmentPriceReport
    {
        public string ContractNumber { get; set; }//số hợp đồng
        public DateTime DateNumber { get; set; }//ngày hợp đồng
        public string CustomerName { get; set; }
        public string TdcApartmentName { get; set; }
        public bool Corner { get; set; } // lô góc
        public string TdcBlockHouseName { get; set; }
        public string TdcLandName { get; set; }
        public string Floor1 { get; set; }//id lầu
        public string TdcFloorName { get; set; }
        public List<ReportIngre> reportIngreTemporarys { get; set; }
        public List<ReportPat> reportPatTemporarys { get; set; } //Tam thoi
        public decimal? TemporaryTotalArea { get; set; }//Diện tích tạm thời
        public decimal? TemporaryTotalPrice { get; set; }//Thành tiền tạm thời
        public string TemporaryDecreeNumber { get; set; }//số quyết định tạm thời
        public DateTime? TemporaryDecreeDate { get; set; }//ngày quyết định tạm thời
        public DateTime FirstPayDate { get; set; }// ngày tiền trả lần đầu
        public List<ReportIngre> reportIngres { get; set; }
        public decimal TotalArea { get; set; }//Diện tích chính thức
        public decimal TotalPrice { get; set; }//Thành tiền chính thức
        public List<ReportPat> reportPats { get; set; } //Chinh thuc
        public string DecreeNumber { get; set; }//số quyết định chính thức
        public DateTime DecreeDate { get; set; }//ngày quyết định chinh thuc
        public List<PayDetail> payDetails { get; set; }
        public decimal OriginalPrecent { get; set; }//% gốc chính thức
        public decimal OriginalPrecentTemprorary { get; set; }//% gốc tạm thời
        public decimal PrincipalPaid { get; set; }//Tiền gốc
        public decimal VATTt { get; set; }
        public decimal VATCt { get; set; }
        public List<TaxEx> taxes { get; set; }
        public List<ReportPat> PatReportTemporary { get; set; } //Tam thoi
        public List<ReportPat> PatReport { get; set; } //Chinh thuc
        public List<ReportIngre> IngreReport { get; set; }
        public List<ReportIngre> IngreReportTemporarys { get; set; }
    }

    public class ReportIngre
    {
        public int Id { get; set; }
        public double Value { get; set; }// Hệ số 
        public int Location { get; set; }
        public string IngrePriceName { get; set; }
        public decimal Area { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }// Tiền thành phần giá bán cấu thành
    }
    public class ReportPat
    {
        public string PATName { get; set; }
        public int Location { get; set; }
        public double Value { get; set; }//giá trị thành phần giá gốc thuế phí
        public decimal PercentTemporary { get; set; }
        public decimal PriceTemporary { get; set; }
        public decimal Percent { get; set; }
        public decimal Price { get; set; }
        public List<ReportIngre> reportIngres { get; set; }
        public List<ReportIngre> IngreReport { get; set; }

    }
    public class PayDetail
    {
        public int index { get; set; }
        public DateTime? PayDateDefault { get; set; } //Ngày Thanh Toán Theo QĐ
        public DateTime? PayDateReal { get; set; } // Ngày Thanh Toán Thực Tế
        public decimal PayAnnual { get; set; }//Số tiền gốc phải trả hàng năm
        public decimal? TotalInterest { get; set; }//Lãi phát sinh theo ngày và tháng 
        public decimal? Fines { get; set; }//Tiền phạt
        public decimal OriginalMoney { get; set; }//Tiền gốc
        public decimal OverPay { get; set; } //Nop du
        public decimal PrinCipal { get; set; } //%goc
        public decimal VAT { get; set; } //%goc

        public List<PayDetailChild> payDetailChilds { get; set; }
    }
    public class PayDetailChild
    {
        public string Name { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class TaxEx
    {
        public int Year { get; set; }
        public decimal Price { get; set; }
    }

    public class Report
    {
        public int? index { get; set; }
        public DateTime? PayDateReal { get; set; } // Ngày Thanh Toán Thực Tế
        public string ContractNumber { get; set; }//số hợp đồng
        public DateTime? ContractDate { get; set; }//ngày hợp đồng
        public string Name { get; set; }
        public string? TdcApartmentName { get; set; }
        public string? TdcBlockHouseName { get; set; }
        public string? TdcLandName { get; set; }
        public decimal OriginalPrice { get; set; } // Giá gốc căn hộ
        public decimal BeforeTax { get; set; } //Giá căn hộ trước thuế
        public decimal PrincipalReceived { get; set; } //Số tiền gốc đã thu
        public decimal InterestRecevied { get; set; } //Số tiền lãi đã thu
        public decimal PrincipalBfTax { get; set; } // Số tiền gốc trước thuế
        public decimal InterestBfTax { get; set; } // Số tiền lãi trước thuế
        public  decimal Principal { get; set; } // Giá gốc căn hộ nộp ngân sách
        public decimal VATPrincipal { get; set; } // VAT gốc đã thu
        public decimal VATInterest { get; set; } // VAT lãi đã thu
        public decimal VATTotal { get; set; } // VAT tổng từng dòng
        public List<ReportPat> Pat { get; set; } 
        public  List<ValuePat> valuePrincipal { get; set; } // value từng cột value ở tiền gốc đã tu
        public List<ValuePat> valueBeforeTax { get; set; } // value từng cột value ở lãi gốc đã tu
        public List<ValuePat> values { get; set; } // value từng cột value ở tiền đã tu
        public List<ValueTotal> valueTotal { get; set; } 
        public string Note { get; set; }
        public byte CheckTotal { get; set; }// 1-tổng gốc và tổng;2-Tổng cộng;3-Số tiền còn lại
        public bool CheckPublicPay { get; set; }
    }
    public class ValuePat
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
    public class ValueTotal // tổng các cột 
    {
        public string Name { get; set; }
        public decimal ValuePrincipal { get; set; }
        public decimal ValueInterest { get; set; }
        public decimal Value { get; set; }
    }

    public class ValueAndTotal
    {
        public DateTime? PayDateReal { get; set; } // Ngày Thanh Toán Thực Tế
        public string ContractNumber { get; set; }//số hợp đồng
        public DateTime? ContractDate { get; set; }//ngày hợp đồng
        public string TemporaryDecreeNumber { get; set; }//số quyết định tạm thời
        public string DecreeNumber { get; set; }//số quyết định chính thức
        public DateTime? TemporaryDecreeDate { get; set; }//ngày quyết định tạm thời
        public DateTime DecreeDate { get; set; }//ngày quyết định chinh thuc
        public string LandName { get; set; }// Tên Lô
        public decimal TotalOrigin { get; set; }
        public decimal ValueIngredient { get; set; }
        public decimal TotalSellCt { get; set; }//tổng giá bán chính thức
        public decimal TotalSellTt { get; set; }//tổng giá bán tạm thời
        public decimal TotalCt { get; set; }//tổng Giá gốc chính thức
        public decimal TotalTt { get; set; }//tổng giá gốc tạm thời
        public decimal sumPatCt { get; set; }// tổng giá thành phần giá gốc thuế phí
        public double PercentSell { get; set; }// % giá bán
        public double PercentOrigin { get; set; }//% giá gốc
        public double PercentIngre { get; set; }//% giá thành phần gốc thuế phí
        public List<ReportPat> PatReportTemporary { get; set; } //Tam thoi
        public List<ReportPat> PatReport { get; set; } //Chinh thuc
        public List<ReportIngre> IngreReport { get; set; }
        public List<ReportIngre> IngreReportTemporarys { get; set; }        
    }
}

