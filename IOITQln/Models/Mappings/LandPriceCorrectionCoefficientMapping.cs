using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LandPriceCorrectionCoefficientMapping : Profile
    {
        public LandPriceCorrectionCoefficientMapping()
        {
            CreateMap<LandPriceCorrectionCoefficient, LandPriceCorrectionCoefficientData>();
            CreateMap<LandPriceCorrectionCoefficientData, LandPriceCorrectionCoefficient>();
        }
    }
}
