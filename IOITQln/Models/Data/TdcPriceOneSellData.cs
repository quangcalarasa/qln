using IOITQln.Entities;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TdcPriceOneSellData : TdcPriceOneSell
    {
        public string TdcCustomerName { get; set; }
        public string TdcProjectName { get; set; }
        public string TdcLandName { get; set; }
        public string TdcBlockHouseName { get; set; }
        public string TdcFloorName { get; set; }
        public string TdcApartmentName { get; set; }
        public string TdcPlatformName { get; set; }


        public List<TdcPriceOneSellTax> tdcPriceOneSellTaxes { get; set; }
        public List<TdcPriceOneSellOfficialData> tdcPriceOneSellOfficials { get; set; }
        public List<TdcPriceOneSellTemporaryData> tdcPriceOneSellTemporaries { get; set; }
        
    }
    public class TdcPriceOneSellEx
    {
        public string? Code { get; set; }// Số hợp đồng
        public DateTime? ActualPaymentDate { get; set; } // Ngày Thanh Toán Thực Tế
        public DateTime? ContractDate { get; set; } //Ngày Kí Hợp Đồng
        public string CustomerName { get; set; } // Tên KH
        public string ProjectName { get; set; } //Tên Dự Án
        public string LandName { get; set; }    // Tên Lô
        public string BlockHouseName { get; set; } // tên Khối
        public string FloorName { get; set; } //Tên Tầng
        public string ApartmentName { get; set; } // Tên Căn
        public string PlatformName { get; set; } // Tên nền đất
        public bool? Corner { get; set; } // Lô góc
        public string? Floor1 { get; set; } // Tên lầu
        public decimal TotalAreaTT { get; set; } // Tổng Diện Tích TT
        public decimal TotalPirceTT { get; set; } // Tổng Thành Tiền TT
        public decimal TotalAreaCT { get; set; } // Tổng Diện Tích CT
        public decimal TotalPirceCT { get; set; } //Tổng Thành Tiền CT
        public decimal TotalPirceCL { get; set; } //Tổng Thành Tiền CL
        public List<OneSellIngreData> temporaryDatas { get; set; }
        public List<OneSellIngreData> officialDatas { get; set; }
        public List<OneSellIngreData> differenceDatas { get; set; }
		//Qfix
		//public int DecisionNumberTT { get; set; } // Số Quyết Định Tam Thời
        public string DecisionNumberTT { get; set; } // Số Quyết Định Tam Thời
        public DateTime DecisionDateTT { get; set; } // Ngày Quyết Định Tam Thời
        public int DecisionNumberCT { get; set; } // Số Quyết Định Chính Thức
        public DateTime DecisionDateCT { get; set; } // Ngày Quyết Định Chính Thức
        public DateTime? FistPayment { get; set; } //Ngày Thanh Toán Lần Đầu
        public decimal MoneyPrincipalTT { get; set; }// Tiền gốc TT
        public decimal MoneyPrincipalCT { get; set; }// Tiền gốc CT
        public decimal MoneyPrincipalCL { get; set; }// Tiền gốc CL
        public decimal AmountPayment { get; set; } //Số tiền phải nộp chính thức
        public decimal AmountPaymentTT { get; set; } //Số tiền phải nộp tạm thời
        public decimal AmountPaymentCL { get; set; } //Số tiền phải nộp chênh lệch
        public decimal PersonalTax { get; set; }// Thuế thu nhập cá nhân
        public decimal RegistrationTax { get; set; }//Thuế trước bạ
        public decimal PrincipalPaid { get; set; }//Tiền gốc đã nộp chính thức
        public decimal PrincipalPaidTT { get; set; }//Tiền gốc đã nộp tạm thời
        public decimal PrincipalPaidCL { get; set; }//Tiền gốc đã nộp chênh lệch
        public decimal PaymentPublic { get; set; }// Tiền công ích
        public decimal PaymentCenter { get; set; }//Tiền nộp về trung tâm
        public decimal Vat { get; set; } //Tính vat chính thức
        public decimal VatTT { get; set; } //Tính vat tạm thời
        public decimal VatCL { get; set; } //Tính vat chênh lệch
        public List<OneSellPriceAndTax> priceTaxes { get; set; }
        public List<OneSellPriceAndTaxTT> priceTaxTTs { get; set; }
        public List<OneSellPriceAndTaxCL> priceTaxCLs { get; set; }
        public List<Oenselltax> oneselltaxes { get; set; }
        public TdcPriceOneSellEx(string code, string customerName, 
                                 string projectName, string laneName, 
                                 string blockHouseName, string floorName, 
                                 string apartmentName, string platformName, bool conner, 
                                 string floor1, decimal totalAreaTT, decimal totalPriceTT, 
                                 decimal totalAreaCT, decimal totalPriceCT,
                                 decimal amountPayment, decimal amountPaymentTT, decimal amountPaymentCL, decimal personalTax, decimal registrationTax,
                                 decimal principalPaid, decimal principalPaidTT, decimal principalPaidCL, decimal paymentPublic, decimal paymentCenter) 
        {
            Code = code;
            CustomerName = customerName;
            ProjectName = projectName;
            LandName = laneName;
            BlockHouseName = blockHouseName;
            FloorName = floorName;
            ApartmentName = apartmentName;
            PlatformName = platformName;
            Corner = conner;
            Floor1 = floor1;
            TotalAreaTT = totalAreaTT;
            TotalPirceTT = totalPriceTT;
            TotalAreaCT = totalAreaCT;
            TotalPirceCT = totalPriceCT;
            AmountPayment = amountPayment;
            AmountPaymentTT = amountPaymentTT;
            AmountPaymentCL = amountPaymentCL;
            PersonalTax = personalTax;
            RegistrationTax = registrationTax;
            PrincipalPaid = principalPaid;
            PrincipalPaidTT = principalPaidTT;
            PrincipalPaidCL = principalPaidCL;
            PaymentPublic = paymentPublic;
            PaymentCenter = paymentCenter;
            temporaryDatas = new List<OneSellIngreData>();
            officialDatas = new List<OneSellIngreData>();
            differenceDatas = new List<OneSellIngreData>();
        }
        public TdcPriceOneSellEx() { }
    }

    public class OneSellIngreData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Area { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
        public double Value { get; set; }

    }
    public class OneSellPriceAndTax
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Location { get; set; }
        public decimal TotalCt { get; set; }
        public decimal Price { get; set; }

        public List<OneSellIngreData> datas { get; set; }

    }
    public class OneSellPriceAndTaxTT
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Location { get; set; }
        public decimal TotalTt { get; set; }
        public decimal Price { get; set; }

        public List<OneSellIngreData> temporaryDatas { get; set; }
    }

    public class OneSellPriceAndTaxCL
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public decimal TotalCl { get; set; }
        public decimal Price { get; set; }
        public List<OneSellIngreData> differenceDatas { get;set; }
    }

    public class Oenselltax
    {

        public int Year { get; set; }
        public decimal Total { get; set; }
    }

    public class GrOneSellDataOff
    {
        public int TdcPriceOneSellId { get; set; }
        public DateTime UpdateTime { get; set; }

        public List<TdcPriceOneSellOfficialData> grData { get; set; }
    }

}
