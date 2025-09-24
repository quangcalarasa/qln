using IOITQln.Entities;

namespace IOITQln.Models.Data
{
    public class ConversionData : Conversion
    {
        public string type_Qd { get; set; }
        public string Coefficient_Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
