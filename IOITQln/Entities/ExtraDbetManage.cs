using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class ExtraDbetManage : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string House { get; set; }
        public string Address { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Period { get; set; }
        public decimal Price { get; set; }
        public DateTime DatePay { get; set; }
        public int DaysOverdue { get; set; }
    }
}
