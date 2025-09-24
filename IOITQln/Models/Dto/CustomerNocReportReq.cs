using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Dto
{
    public class CustomerNocReportReq
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string IdentityCode { get; set; }
        public TypeSex? Sex { get; set; }
    }
}
