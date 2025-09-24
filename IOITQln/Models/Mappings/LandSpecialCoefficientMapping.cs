using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LandSpecialCoefficientMapping : Profile
    {
        public LandSpecialCoefficientMapping()
        {
            CreateMap<LandSpecialCoefficient, LandSpecialCoefficientData>();
            CreateMap<LandSpecialCoefficientData, LandSpecialCoefficient>();
        }
    }
}
