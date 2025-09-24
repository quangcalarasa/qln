using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Coefficient : AbstractEntity<int>
    {
        public int UnitPriceId { get; set; }
        public double Value { get; set; } //Giá trị
        public DateTime DoApply { get; set; } // Ngày Áp Dụng
        public string Note { get; set; } // Diễn giải
    }
}
