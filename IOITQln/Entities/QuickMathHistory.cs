using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class QuickMathHistory : AbstractEntity<int>
    {
        public DateTime DoApply { get; set; } //Ngày áp dụng
        public decimal Value { get; set; } //Giá trị thay đổi
        public string TypeValue { get; set; }  //Loại giá trị thay đổi
        public int Type { get; set; } //Kiểu giá trị thay đổi 1-VAT,2-LCB,3-Giá chuẩn,4-thời điểm bố trí
        public double OldValue { get; set; }
        public int StatusProcess { get; set; } //1-Chờ ;2- Hoàn thành
    }
}
