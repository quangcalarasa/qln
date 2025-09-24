using IOITQln.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Dto
{
    public class CustomerNocReportRes: Customer
    {
        public string ContractCode { get; set; }
        public DateTime? ContractDate { get; set; }
    }
}
