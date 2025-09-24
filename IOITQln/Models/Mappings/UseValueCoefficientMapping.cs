using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class UseValueCoefficientMapping : Profile
    {
        public UseValueCoefficientMapping()
        {
            CreateMap<UseValueCoefficient, UseValueCoefficientData>();
            CreateMap<UseValueCoefficientData, UseValueCoefficient>();
        }
    }
}
