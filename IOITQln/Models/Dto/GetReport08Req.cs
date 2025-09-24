using System;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Models.Dto
{
    public class GetReport08Req
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomertID { get; set; }
        public string? HouseCode { get; set; }
        public int? Md167TransferUnitId { get; set; }
        public int? DistrictId { get; set; } // Quận.
        public int? WardId { get; set; } // Xã.
        public int? LaneId { get; set; } // Đường.
        public int? StatusOfUse { get; set; }// Hiện trạng sử dụng đất
        public int? HouseId { get; set; }
    }
}
