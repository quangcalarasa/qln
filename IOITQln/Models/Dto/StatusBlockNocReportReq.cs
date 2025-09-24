using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Dto
{
    public class StatusBlockNocReportReq
    {
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
        public string Code { get; set; }
        public UsageStatus? UsageStatus { get; set; }
    }
}
