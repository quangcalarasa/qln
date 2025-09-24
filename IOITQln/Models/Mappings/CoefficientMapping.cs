using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class CoefficientMapping : Profile
    {
        public CoefficientMapping()
        {
            CreateMap<Coefficient, CoefficientData>();
            CreateMap<CoefficientData, Coefficient>();
        }
    }
}
