using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class LandMapping : Profile
    {
        public LandMapping() 
        { 
            CreateMap<Land, LandData>();
            CreateMap<LandData, Land>();
        }
    }
}
