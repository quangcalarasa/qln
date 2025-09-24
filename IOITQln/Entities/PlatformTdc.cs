using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PlatformTdc: AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? TDCProjectId { get; set; }
        public int? LandId { get; set; }
        public bool? Corner { get; set; }
        public int Platcount { get; set; }// số nền đất
        public double? LengthArea { get; set; }// chiều dài
        public double? WidthArea { get ; set; }//chiều rộng
        public double? LandArea { get; set; }
        public string Note { get; set; }
    }
}
