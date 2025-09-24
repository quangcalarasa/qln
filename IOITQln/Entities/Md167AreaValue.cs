using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    //Hệ số khu vuc
    public class Md167AreaValue : AbstractEntity<int>
    {
        public string Name { get; set; } // Tên khu vực.
        public decimal Value { get; set; } // Hệ số.
        public string Decision { get; set; } // Quyết định.
        public DateTime EffectiveTime { get; set; } // Thời gian hiệu lực.
        public string LandPurpose { get; set; } // Mục đích sử dụng đất.
    }
}
