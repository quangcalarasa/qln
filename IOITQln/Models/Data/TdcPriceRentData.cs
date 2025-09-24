using Castle.Core;
using IOITQln.Entities;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class TdcPriceRentData : TdcPriceRent
    {
        public string TdcCustomerName { get; set; }
        public string TdcProjectName { get; set; }
        public string TdcLandName { get; set; }
        public string TdcBlockHouseName { get; set; }
        public string TdcFloorName { get; set; }
        public string TdcApartmentName { get; set; }
        public bool? Corner { get; set; }
        public List<TdcPriceRentTax> tdcPriceRentTaxes { get; set; }
        public List<TdcPriceRentTemporaryData> tdcPriceRentTemporaries { get; set; }
        public List<TdcPriceRentOfficialData> tdcPriceRentOfficials { get; set; }
        public List<TdcPriceRentExcel> tdcPriceRentExcels { get; set; }
        public List<TdcPriceRentReport> tdcPriceRentReports { get; set; }

        public List<TdcPriceRentExcelGroupByPayTimeId> tdcPriceRentExcelGroups { get; set; }
    }

    public class TdcPriceRentTemporaryData : TdcPriceRentTemporary
    {
        public string IngrePriceName { get; set; }
        public double Value { get; set; }
    }
    public class TdcPriceRentOfficialData : TdcPriceRentOfficial
    {
        public string IngrePriceName { get; set; }
        public double Value { get; set; }


    }

    public class TdcPriceRentExcel
    {
        public int? PayTimeId { get; set; }
        public int PayCount { get; set; }
        public string? PaymentTimes { get; set; } // Lần Trả
        public DateTime? PaymentDatePrescribed { get; set; } //Ngày Thanh Toán Theo QĐ
        public DateTime? PaymentDatePrescribed1 { get; set; } //Ngày Thanh toán Dự Kiến
        public int Year { get; set; } //Năm
        public DateTime? ExpectedPaymentDate { get; set; } // Ngày Thanh Toán Thực Tế
        public int? DailyInterest { get; set; }//Thời gian tính lãi theo ngày
        public double? DailyInterestRate { get; set; } //Lãi suất tính theo ngày
        public decimal? UnitPay { get; set; }//Số tiền phải trả từng tháng 
        public decimal? PriceEarnings { get; set; } //Số tiền lãi phát sinh do chậm thanh toán 
        public decimal? PricePaymentPeriod { get; set; } //Số tiền đến kỳ phải thanh toán
        public decimal? Pay { get; set; } //Số tiền phải thanh toán
        public decimal? Paid { get; set; } //Số tiền đã thanh toán
        public decimal? PriceDifference { get; set; } // Số tiền chênh lệch
        public byte? TypeRow { get; set; } // Loại row: Tiền thế chân = 1, Năm thứ n = 2, Kỳ thanh toán = 3, Nợ cũ = 4
        public bool? Status { get; set; } //đã thanh toán : true,chưa thanh toán : false
        public string Note { get; set; } // Ghi Chú
        public bool Check { get; set; }
        public TypePayQD? RowStatus { get; set; }
        public bool? PricePublic { get; set; }
        public bool IsImport { get; set; }

        public TdcPriceRentExcel()
        {
            Check = false;
            IsImport = false;
        }
        public TdcPriceRentExcel(int? payTimeId, bool status, string paymentTimes, DateTime? paymentDatePrescribed,
             DateTime? paymentDatePrescribed1,
            int year, DateTime? expectedPaymentDate, string note, int? dailyInterest,
            double? dailyInterestRate, decimal? unitPay, decimal? priceEarnings, decimal? pricePaymentPeriod,
            decimal? pay, decimal? paid, decimal? priceDifference, byte? typeRow,bool check,bool isImport)
        {
            PayTimeId = payTimeId;
            this.Status = status;
            PaymentTimes = paymentTimes;
            PaymentDatePrescribed = paymentDatePrescribed;
            PaymentDatePrescribed1 = paymentDatePrescribed1;
            Year = year;
            ExpectedPaymentDate = expectedPaymentDate;
            Note = note;
            DailyInterest = dailyInterest;
            DailyInterestRate = dailyInterestRate;
            UnitPay = unitPay;
            PriceEarnings = priceEarnings;
            PricePaymentPeriod = pricePaymentPeriod;
            Pay = pay;
            Paid = paid;
            PriceDifference = priceDifference;
            TypeRow = typeRow;
            Check = check;
            IsImport = isImport;
        }
    }
    public class TdcPriceRentExcelGroupByPayTimeId
    {
        public bool? Status { get; set; }
        public Object? PayTimeId { get; set; }
        public decimal? Pay { get; set; }
        public decimal? Paid { get; set; }
        public decimal? PriceDifference { get; set; }
        public bool? PricePublic { get; set; }
        public bool Check { get; set; }

        public List<TdcPriceRentExcel> tdcPriceRentExcels { get; set; }
    }
    public class TdcPriceRentMetaData : TdcPriceRentExcelMeta
    {
        public List<TdcPriceRentExcelData> tdcPriceRentExcelDatas { get; set; }

    }
    public class TdcPriceRentReport
    {
        public string? Code { get; set; }// Số hợp đồng
        public DateTime? ContractDate { get; set; } //Ngày Kí Hợp Đồng
        public string CustomerName { get; set; } // Tên KH
        public string ProjectName { get; set; } //Tên Dự Án
        public string LandName { get; set; }    // Tên Lô
        public string BlockHouseName { get; set; } // tên Khối
        public string FloorName { get; set; } //Tên Tầng
        public string ApartmentName { get; set; } // Tên Căn
        public bool? Corner { get; set; } // Lô góc
        public string? Floor { get; set; } // Tên lầu
        public decimal? TotalAreaTT { get; set; } // Tổng Diện Tích TT
        public decimal? TotalPirceTT { get; set; } // Tổng Thành Tiền TT
        public decimal TotalAreaCT { get; set; } // Tổng Diện Tích CT
        public decimal TotalPirceCT { get; set; } //Tổng Thành Tiền CT
        public List<IngreData> temporaryDatas { get; set; }
        public List<IngreData> officialDatas { get; set; }
        public string DecisionNumberTT { get; set; } // Số Quyết Định Tam Thời
        public DateTime? DecisionDateTT { get; set; } // Ngày Quyết Định Tam Thời
        public string DecisionNumberCT { get; set; } // Số Quyết Định Chính Thức
        public DateTime DecisionDateCT { get; set; } // Ngày Quyết Định Chính Thức
        public DateTime? FistPayment { get; set; } //Ngày Thanh Toán Lần Đầu
        public decimal PercentOriginTT { get; set; } // %gốc
        public decimal PercentOriginCT { get; set; } // %gốc CT
        public List<PriceAndTax> priceAndTaxes { get; set; }
        public List<PriceAndTaxTT> priceAndTaxTTs { get; set; }
        public List<Excel> excels { get; set; }
        public List<Tax> taxes { get; set; }

        public TdcPriceRentReport(string code,string customerName,string projectName,string laneName,string blockHouseName,string floorName,string apartmentName,bool conner,string floor, decimal totalAreaTT,decimal totalPriceTT, decimal totalAreaCT, decimal totalPriceCT)
        {
            Code = code;
            CustomerName = customerName;
            ProjectName = projectName;
            LandName = laneName;
            BlockHouseName = blockHouseName;
            FloorName = floorName;
            ApartmentName = apartmentName;
            Corner = conner;
            Floor = floor;
            TotalAreaTT = totalAreaTT;
            TotalPirceTT = totalPriceTT;
            TotalAreaCT = totalAreaCT;
            TotalPirceCT = totalPriceCT;
            temporaryDatas = new List<IngreData>();
            officialDatas = new List<IngreData>();

        }
        public TdcPriceRentReport() { }
    }

    public class IngreData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Area { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
        public double Value { get; set; }

    }
    public class PriceAndTax
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Location  { get; set; }
        public decimal Total { get; set; }
        public List<IngreData> datas { get; set; }
    }
    public class PriceAndTaxTT
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Location { get; set; }
        public decimal Total { get; set; }
        public List<IngreData> temporaryDatas { get; set; }
    }
    public class Excel
    {
        public  string index { get; set; }
        public DateTime? PaymentDate { get; set; } //Ngày Phải Nộp Tiền
        public DateTime? PayDate { get; set; } // Ngày nộp tiền
        public decimal? AmountPayable { get; set; } //Số tiền phải nộp
        public decimal? PrinCipal { get; set; } // Tiền  gốc
        public decimal Interest { get; set; } // Tiền Lãi
        public decimal PricePunish { get; set; } // tiền phạt
        public decimal? Overpayment { get; set; } // Tiền dư
        public decimal? VAT { get; set; } // Tiền dư
        public List<childExcel> excelChilds { get; set; }
    }
    public class childExcel
    {
        public string Name { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class Tax
    {
        public int Year { get; set; }
        public decimal Price { get; set; }
    }

    public class GrDataOff<TList>
    {
        public DateTime UpdateTime { get; set; }

        public List<TList> grData { get; set; }
    }
}

