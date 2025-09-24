using System;

namespace IOITQln.Models.Dto
{
    public class SynthesisReportNocReq
    {
        public string Code { get; set; }
        public string CustomerName { get; set; }
        public string IdentityCode { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
        public bool? HasSaleInfo { get; set; }
        public bool? HasRentInfo { get; set; }
    }
}
