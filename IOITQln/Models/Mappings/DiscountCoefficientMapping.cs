using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class DiscountCoefficientMapping : Profile
    {
        public DiscountCoefficientMapping()
        {
            CreateMap<DiscountCoefficient, DiscountCoefficientData>();
            CreateMap<DiscountCoefficientData, DiscountCoefficient>();
        }
    }
}
