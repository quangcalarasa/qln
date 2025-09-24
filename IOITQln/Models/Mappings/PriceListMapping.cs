using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class PriceListMapping : Profile
    {
        public PriceListMapping()
        {
            CreateMap<PriceList, PriceListData>();
            CreateMap<PriceListData, PriceList>();
        }
    }
}
