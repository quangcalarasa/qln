using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TDCProject : AbstractEntity<int>
    {
        public string? Code { get; set; }
        public string Name { get; set; }
        public int LandCount  {get; set; }
        public string FullAddress { get; set; }
        public string HouseNumber { get; set; }
        public int? Lane { get; set; }
        public int? Ward { get; set; }
        public int District { get; set; }
        public int Province { get; set; }
        public string BuildingName { get; set; }
        public double TotalAreas { get; set; }
        public int TotalApartment { get; set; }
        public int TotalPlatform { get; set; }
        public double TotalFloorAreas { get; set; }
        public double TotalUseAreas { get; set; }
        public double TotalBuildAreas { get; set; }
        public decimal DebtRate { get; set; }
        public decimal LateRate { get; set; }
        public string Note { get; set; }

    }
}
