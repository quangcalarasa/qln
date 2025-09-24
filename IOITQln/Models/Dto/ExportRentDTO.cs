using System;

namespace IOITQln.Models.Dto
{
    public class ExportRentReq
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? DistrictId { get; set; } // Quận.
        public int? WardId { get; set; } // Xã.
        public int? LaneId { get; set; } // Đường.
        public string? CustomerName { get; set; }
        public string? CodeHS { get; set; }
        public string? Code { get; set; }
    }

    public class ExportRentRes
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? District { get; set; } // Quận.
        public string? Ward { get; set; } // Xã.
        public string? Lane { get; set; } // Đường.
        public string? CustomerName { get; set; }
        public string? CodeHS { get; set; }
        public string? Code { get; set; }
        public Guid?  Id { get; set; }
        public string? Provice { get; set; }
        public DateTime DateAssgint { get; set; }
        public decimal Paid { get; set;  }
        public decimal Total { get; set; }
        public decimal Diff { get; set; }
        public string Address { get; set; }
        public DateTime? PaymentDeadline { get; set; } // Hạn thanh toán
        public int? DateDiff { get; set; } //Thời gian quá hạn
        public DateTime? NearDatePay { get; set; } //Tg thanh toán gần nhất
        public int LandId { get; set; }
        public int WardId { get; set; }
        public int DistrictId { get; set; }
        public int RentApartmentId { get; set; }
        public int RentBlockId { get; set; }
        public DateTime DateEnd { get; set; }
        public string FullAddress { get; set; }
    }


}
