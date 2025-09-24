using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class DeductionCoefficientMapping : Profile
    {
        public DeductionCoefficientMapping()
        {
            CreateMap<DeductionCoefficient, DeductionCoefficientData>();
            CreateMap<DeductionCoefficientData, DeductionCoefficient>();
        }
    }
}
