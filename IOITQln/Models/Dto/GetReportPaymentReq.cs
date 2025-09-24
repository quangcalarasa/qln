using System;

namespace IOITQln.Models.Dto
{
    public class GetReportPaymentReq
    {
        public int? HouseId { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
    }
}
