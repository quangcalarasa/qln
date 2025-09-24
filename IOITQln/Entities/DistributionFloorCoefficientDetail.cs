using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class DistributionFloorCoefficientDetail : AbstractEntity<int> //Chi tiết bảng Hệ số phân bổ các tầng
    {
        public int DistributionFloorCoefficientId { get; set; }
        //public TypeFloor TypeFloor { get; set; }
        public NumberFloor NumberFloor { get; set; }
        public float? Value1 { get; set; }
        public float? Value2 { get; set; }
        public float? Value3 { get; set; }
        public float? Value4 { get; set; }
        public float? Value5 { get; set; }
        public float? Value6 { get; set; }
    }
}
