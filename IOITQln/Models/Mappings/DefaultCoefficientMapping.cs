using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class DefaultCoefficientMapping : Profile
    {
        public DefaultCoefficientMapping()
        {
            CreateMap<DefaultCoefficient, DefaultCoefficientData>();
            CreateMap<DefaultCoefficientData, DefaultCoefficient>();
        }
    }
}
