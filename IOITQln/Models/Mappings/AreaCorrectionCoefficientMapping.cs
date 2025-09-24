using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class AreaCorrectionCoefficientMapping : Profile
    {
        public AreaCorrectionCoefficientMapping()
        {
            CreateMap<AreaCorrectionCoefficient, AreaCorrectionCoefficientData>();
            CreateMap<AreaCorrectionCoefficientData, AreaCorrectionCoefficient>();
        }
    }
}
