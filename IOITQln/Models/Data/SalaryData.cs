using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class SalaryData : Salary
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
