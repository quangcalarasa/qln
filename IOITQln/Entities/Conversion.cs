using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;
using System;

namespace IOITQln.Entities
{
    public class Conversion : AbstractEntity<int>
    {
        public TypeQD TypeQD { get; set; } 
        public TypeTable CoefficientName { get; set; } //Tên hệ số
        public string Code { get; set; } // ký hiệu
        public string Note { get; set; } // Diễn giải
    }
}