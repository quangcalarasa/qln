using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Dto
{
    public class StatusBlockNocReportRes
    {
        public string Code { get; set; }
        public string Address { get; set; }
        public string Lane { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string UsageStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
