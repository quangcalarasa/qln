using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class QuickMathLog : AbstractEntity<int>
    {
        public string CodeHD { get; set; }  //Loại giá trị thay đổi
        public int StatusProcess { get; set; } //1-Thất bại ;2- Hoàn thành
        public int QuickMathHistoryId { get; set; } //Id 
    }
}
