using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class LandSpecialCoefficient : AbstractEntity<int>           //Hệ số khu đất, thửa đất có hình thể đặc biệt
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public DateTime? DoApply { get; set; }        //Ngày áp dụng
        public double Value1 { get; set; }    //Giá trị - Có phần S không mặt tiền đường (hẻm) từ 15m2 trở lên
        public double Value2 { get; set; }    //Giá trị - 5R< D <=8R
        public double Value3 { get; set; }    //Giá trị - D >8R
        public double Value4 { get; set; }    //Giá trị - Đoạn đường nằm hai bên dạ cầu (song song cầu)
        public double Value5 { get; set; }    //Giá trị - Đoạn đường nằm hai bên cầu vượt (song song cầu)
        public double Value6 { get; set; }    //Giá trị - Nằm trong hành lang bảo vệ của đường điện cao thế
        public double Value7 { get; set; }    //Giá trị - Đường nhánh dẫn lên cầu vượt 
        public double Value8 { get; set; }    //Giá trị - Cách lề đường bằng một con kênh, rạch không được san lấp

    }
}
