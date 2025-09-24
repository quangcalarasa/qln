using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LandPriceMapping : Profile
    {
        public LandPriceMapping()
        {
            CreateMap<LandPrice, LandPriceData>();
            CreateMap<LandPriceData, LandPrice>();
        }
    }
}
