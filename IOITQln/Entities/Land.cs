using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Land : AbstractEntity<int> // Lô tái định cư
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? TDCProjectId { get; set; }
        public int BlockHouseCount { get; set; }
        public double? TotalArea { get; set; } // Tổng diện tích
        public double? ConstructionApartment { get; set; } // Tổng số căn hộ
        public double? ConstructionLand { get; set; } // Tổng số nền đất
        public double? ConstructionValue { get; set; } // Tổng diện tích sử dụng
        public double? ContrustionBuild { get; set; } // Tổng diện tích xây dựng
        public PlotType? PlotType { get; set; } // Loại lô
        public string Note { get; set; }

    }
}
