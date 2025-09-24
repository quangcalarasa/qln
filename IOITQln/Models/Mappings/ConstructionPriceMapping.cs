using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class ConstructionPriceMapping : Profile
    {
        public ConstructionPriceMapping()
        {
            CreateMap<ConstructionPrice, ConstructionPriceData>();
            CreateMap<ConstructionPriceData, ConstructionPrice>();
        }
    }
}
