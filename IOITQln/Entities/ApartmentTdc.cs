using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class ApartmentTdc:AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? TDCProjectId { get; set; }
        public int? LandId { get; set; }
        public int? BlockHouseId { get; set; }
        public int? FloorTdcId { get; set; }
        public bool? Corner { get; set; }
        public int? RoomNumber { get; set; }// số phòng ngủ
        public double? ConstructionValue { get; set; } // Tổng diện tích sử dụng
        public double? ContrustionBuild { get; set; } // Tổng diện tích xây dựng
        public string Note { get; set; }
    }
}
