using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class UseValueCoefficientItem : AbstractEntity<int>        //Hệ số điều chỉnh giá trị sử dụng
    {
        public int UseValueCoefficientId { get; set; }
        public int FloorId { get; set; }
        public bool? IsMezzanine { get; set; }
        public double Value { get; set; }
        public string Note { get; set; }
    }
}
