using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Models.Data
{
    public class Md167ManagePaymentData: Md167ManagePayment
    {
        public List<HousePaymentData> housePayments { get; set;}
    }

    //Data Nhà dùng trong hợp đồng
    public class Md167HouseManagePaymentEx
    {
        public string Code { get; set; } //mã hợp đồng  
        public DateTime Date { get; set; }// ngày nộp hợp đồng
        public int Year { get; set; }//năm nộp
        public string HouseName { get; set; }
        public decimal? Payment { get; set; }//tổng tiền thuế
        public string Note { get; set; }//ghi chú
        public List<PaymentHouse> paymentHouses { get; set; }
        public Md167HouseManagePaymentEx(string code, int year,
                                         string housename, decimal payment,
                                         string note)
        {
            Code = code;
            Year = year;
            HouseName = housename;
            Payment = payment;
            Note = note;
        }

        public Md167HouseManagePaymentEx() {}
    }
    public class PaymentHouse
    {
        public int HouseId { get; set; } //id nhà
        public decimal? TaxNN { get; set; } // tiền phải đóng
        public decimal? Paid { get; set; } // tiền đã đóng
        public decimal? Debt { get; set; } // tiền còn thiếu
        public string HouseName { get; set; } // Số nhà.
        public string ProviceName { get; set; } // Tỉnh.
        public string DistrictName { get; set; } // Quận.
        public string WardName { get; set; } // Xã.
        public string LaneName { get; set; } // Đường.
    }
}
