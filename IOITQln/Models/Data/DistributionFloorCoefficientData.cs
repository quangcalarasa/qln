using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class DistributionFloorCoefficientData : DistributionFloorCoefficient
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        public List<DistributionFloorCoefficientDetail> distributionFloorCoefficientDetails { get; set; }
    }

    public class FlatCoefficientData
    {
        public int Id { get; set; }
        public int? DecreeType1Id { get; set; }      //Nghị định
        public float? FlatCoefficient { get; set; }     //Hệ số nhà chung cư
        public string Note { get; set; }
    }
}
