using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class BlockHouse : AbstractEntity<int> // Khối nhà tái định cư
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? TDCProjectId { get; set; }
        public int? LandId { get; set; }
        public int FloorTdcCount { get; set; }
        public int TotalApartmentTdcCount { get; set; }
        public double? ConstructionValue { get; set; }
        public double? ContrustionBuild { get; set; }
    }
}
