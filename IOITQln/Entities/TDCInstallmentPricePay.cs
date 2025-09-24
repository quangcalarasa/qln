using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class TDCInstallmentPricePay : AbstractEntity<int>
    {
        public int TdcInstallmentPriceId { get; set; }
        public int PayTime { get; set; }
        public DateTime Date { get; set; } //Ngày Thanh Toán Thực Tế
        public decimal Value { get; set; } //Số tiền đã thanh toán
        public int PayCount { get; set; }
        public bool PublicPay { get; set; }
        public bool IsPayOff { get; set; }
    }
}
