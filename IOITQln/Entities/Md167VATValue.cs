using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    //Thuế VAT
    public class Md167VATValue : AbstractEntity<int>
    {
        public decimal Value { get; set; } // Giá trị.
        public DateTime EffectiveDate { get; set; } // Ngày có hiệu lực.
        public string Note { get; set; } // Ghi chú.
    }
}
