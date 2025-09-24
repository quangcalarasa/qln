using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class LandPriceCorrectionCoefficientData : LandPriceCorrectionCoefficient
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
    }
}
