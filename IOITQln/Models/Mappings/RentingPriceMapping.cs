using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class RentingPriceMapping : Profile
    {
        public RentingPriceMapping()
        {
            CreateMap<RentingPrice, RentingPriceData>();
            CreateMap<RentingPriceData, RentingPrice>();
        }
    }
}
