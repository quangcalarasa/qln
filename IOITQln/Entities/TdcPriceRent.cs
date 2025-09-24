using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRent : AbstractEntity<int>
    {
        public string Code { get; set; } // Số Hợp Đồng
        public DateTime Date { get; set; } // Ngày Kí Hợp Đồng
        public string Floor1 { get; set; } // Lầu
        public int TdcCustomerId { get; set; } 
        public int TdcProjectId { get; set; }
        public int LandId { get; set; }
        public int BlockHouseId { get; set; }
        public int FloorTdcId { get; set; }
        public int TdcApartmentId { get; set; }   
        public DateTime DateTDC { get; set; } // Ngày QĐ bố trí Tái Định Cư
        public DateTime DateTTC { get; set; } // Ngày Nộp Tiền Thế Chân
        public int MonthRent { get; set; }  // Số Tháng Thuê
        public decimal PriceTC { get; set; } // Số tiền thế chân
        public  decimal PriceMonth { get; set; } // Sô tiền nộp hàng tháng
        public decimal PriceToTal { get; set; } // Tổng số tiền phải trả
        public decimal? PriceTT { get; set; } // tiền nộp về trung tâm
        public string DecisionNumberCT { get; set; } // Số quyết định chính thức
        public DateTime DecisionDateCT { get; set; } // Ngày Quyết Định Chính thức
        public string DecisionNumberTT { get; set; } // Số quyết định tạm thời
        public DateTime? DecisionDateTT { get; set; } // Ngày Quyết Định Tạm Thời
        public decimal? TotalAreaTT { get; set; } //Tổng Diện Tích Tạm Thời
        public decimal TotalAreaCT { get; set; } //Tổng Diện Tích Chính Thức
        public decimal? TotalPriceTT { get; set; } //Thành Tiền Tạm Thời
        public decimal TotalPriceCT { get; set; } //Thành Tiền Chính Thức
        public bool Check { get; set; } //Biến để nhét Id
    }
}

