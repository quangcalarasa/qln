using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class PositionCoefficientMapping : Profile
    {
        public PositionCoefficientMapping()
        {
            CreateMap<PositionCoefficient, PositionCoefficientData>();
            CreateMap<PositionCoefficientData, PositionCoefficient>();
        }
    }
}
