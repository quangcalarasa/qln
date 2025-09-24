using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LandscapeLimitMapping : Profile
    {
        public LandscapeLimitMapping()
        {
            CreateMap<LandscapeLimit, LandscapeLimitData>();
            CreateMap<LandscapeLimitData, LandscapeLimit>();
        }
    }
}
