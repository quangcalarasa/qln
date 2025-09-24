using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class DistributionFloorCoefficient : AbstractEntity<int> //Hệ số phân bổ các tầng
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public bool? ApplyMezzanineCoefficient { get; set; } //Áp dụng hệ số tầng lửng
        public float? MezzanineCoefficient { get; set; }     //Hệ số tầng lửng
        public float? FlatCoefficient { get; set; }     //Hệ số nhà chung cư
    }
}
