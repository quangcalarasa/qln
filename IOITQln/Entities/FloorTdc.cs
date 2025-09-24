using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class FloorTdc: AbstractEntity<int> // Tầng tái định cư
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? FloorNumber { get; set; }
        public int? TDCProjectId { get; set; }
        public int? LandId { get; set; }
        public int? BlockHouseId { get; set; }
        public int ApartmentTdcCount { get; set; }
        public double? ConstructionValue { get; set; } // Tổng diện tích sử dụng
        public double? ContrustionBuild { get; set; } // Tổng diện tích xây dựng
        public string Note { get; set; }
    }
}
