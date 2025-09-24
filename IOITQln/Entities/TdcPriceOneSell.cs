using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceOneSell : AbstractEntity<int>
    {
        public string Code { get; set; } // Số hợp đồng
        public DateTime Date { get; set; } // Ngày Kí Hợp Đồng
        public string Floor1 { get; set; } // Lầu
        public int TdcCustomerId { get; set; }
        public int TdcProjectId { get; set; }
        public int LandId { get; set; }
        public int BlockHouseId { get; set; }
        public int FloorTdcId { get; set; }
        public int PlatformId { get; set; }
        public int TdcApartmentId { get; set; }
        public bool Corner { get; set; }
        public decimal PersonalTax { get; set; }
        public decimal RegistrationTax { get; set; }
        public int DecisionNumberCT { get; set; } // Số quyết định chính thức
        public DateTime DecisionDateCT { get; set; } // Ngày Quyết Định Chính thức
		//Qfix
		//public int DecisionNumberTT { get; set; } // Số quyết định tạm thời
        public string DecisionNumberTT { get; set; } // Số quyết định tạm thời
        public DateTime DecisionDateTT { get; set; } // Ngày Quyết Định Tạm Thời
        public decimal TotalAreaTT { get; set; } //Tổng Diện Tích Tạm Thời
        public decimal TotalAreaCT { get; set; } //Tổng Diện Tích Chính Thức
        public decimal TotalPriceTT { get; set; } //Thành Tiền Tạm Thời
        public decimal TotalPriceCT { get; set; } //Thành Tiền Chính Thức
        public decimal PaymentPublic { get; set; }// Tiền công ích
        public decimal PaymentCenter { get; set; }//Tiền nộp về trung tâm
        public bool check { get; set; }// check

    }
}
