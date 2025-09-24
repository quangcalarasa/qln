using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LaneMapping : Profile
    {
        public LaneMapping()
        {
            CreateMap<Lane, LaneData>();
            CreateMap<LaneData, Lane>();
        }
    }
}
