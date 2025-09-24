using System;

namespace IOITQln.Models.Dto
{
    public class GetReport07Req
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomertID { get; set; }
        public string? HouseCode { get; set; }
        public int? DistrictId { get; set; } // Quận.
        public int? WardId { get; set; } // Xã.
        public int? LaneId { get; set; } // Đường.
        public int? HouseId { get; set; }
    }
}
