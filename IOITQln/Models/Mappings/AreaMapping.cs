using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class AreaMapping : Profile
    {
        public AreaMapping()
        {
            CreateMap<Area, AreaData>();
            CreateMap<AreaData, Area>();
        }
    }
}
