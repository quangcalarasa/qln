using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class BlockDetail : AbstractEntity<long>         //Chi tiết thông tin tầng của căn nhà
    {
        public int BlockId { get; set; }
        public int FloorId { get; set; }
        public int AreaId { get; set; }
        public int? Level { get; set; }
        public float? TotalAreaFloor { get; set; }
        public float? TotalAreaDetailFloor { get; set; }
        public float? GeneralArea { get; set; }
        public float? PrivateArea { get; set; }
        public float? YardArea { get; set; }                    //Diện tích sân chung phân bổ
        public float? Coefficient_99 { get; set; }
        public float? Coefficient_34 { get; set; }
        public float? Coefficient_61 { get; set; }
        public DateTime? DisposeTime { get; set; }              //Thời điểm bố trí
    }
}
