using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LandPriceItemMapping : Profile
    {
        public LandPriceItemMapping()
        {
            CreateMap<LandPriceItem, LandPriceItemData>();
            CreateMap<LandPriceItemData, LandPriceItem>();
        }
    }
}
