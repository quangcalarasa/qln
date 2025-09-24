using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraConfigNotii : AbstractEntity<int>
    {
        public DateTime Date { get; set; }
        public int DayOver { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
    }
}
