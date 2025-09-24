using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraDebtReminder : AbstractEntity<int>
    {
        public int DebtRemindNumber { get; set; }
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public string House { get; set; }
        public string Apartment { get; set; }
        public string Address { get; set; }
        public string Owner { get; set; }
        public string SDT { get; set; }
        public string Content { get; set; }
        public int Times { get; set; }

    }
}
