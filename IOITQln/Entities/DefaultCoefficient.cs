using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class DefaultCoefficient : AbstractEntity<int>
    {
        public int UnitPriceId { get; set; }
        public TypeTable CoefficientName { get; set; } //Tên hệ số
        public double Value { get; set; } //Giá trị
        public string Content { get; set; } // Nội dung
        public string Note { get; set; } // Diễn giải
        public Boolean Status_use { get; set; } //Trạng thái sử dụng mặc định
    }
}
