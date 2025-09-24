using DevExpress.Office.Utils;
using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class DiscountCoefficientData : DiscountCoefficient
    {
        public string UnitPriceName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
