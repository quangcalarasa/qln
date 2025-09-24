using IOITQln.Common.Bases;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Entities
{
    public class HousePayment : AbstractEntity<int>
    {
        public int HouseId { get; set; }
        public int Md167PaymentId { get; set; }
        public decimal? TaxNN { get; set; } // tiền phải đóng
        public decimal? Paid { get; set; } // tiền đã đóng
        public decimal? Debt { get; set; } // tiền còn thiếu
        public string HouseName { get; set; } // Số nhà.
        public string ProviceName { get; set; } // Tỉnh.
        public string DistrictName { get; set; } // Quận.
        public string WardName { get; set; } // Xã.
        public string LaneName { get; set; } // Đường.
        public string fullAddress { get; set; }// Địa chỉ nhà
    }
}
