using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRentPay : AbstractEntity<int>
    {
        public int TdcPriceRentId { get; set; }
        public int PayTime { get; set; }
        public DateTime PaymentDate { get; set; } //Ngày Thanh Toán Thự Tế
        public decimal AmountPaid { get; set; } //Số tiền đã thanh toán
        public int PayCount { get; set; } // Số Kì Thanh Toán
        public bool PricePublic { get; set; } //Tiền Công Ích Thu

    }
}
