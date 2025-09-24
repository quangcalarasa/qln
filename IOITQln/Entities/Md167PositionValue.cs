using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    //Hệ số vị trí
    public class Md167PositionValue : AbstractEntity<int>
    {
        public DecreeEnum decree { get; set; }
        public DateTime DoApply { get; set; } // Lưu thông tin về ngày áp dụng văn bản pháp luật.
        public string Text1 { get; set; }     // Mô tả vị trí 1
        public string Text2 { get; set; }     // Mô tả vị trí 2 
        public string Text3 { get; set; }     // Mô tả vị trí 3 
        public string Text4 { get; set; }     // Mô tả vị trí 4 
        public string Position1 { get; set; } // Lưu thông tin về vị trí 1.
        public string Position2 { get; set; } // Lưu thông tin về vị trí 2.
        public string Position3 { get; set; } // Lưu thông tin về vị trí 3.
        public string Position4 { get; set; } // Lưu thông tin về vị trí 4.
    }
}
