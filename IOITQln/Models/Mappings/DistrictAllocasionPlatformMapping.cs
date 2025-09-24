using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class DistrictAllocasionPlatformMapping : Profile
    {
        public DistrictAllocasionPlatformMapping()
        {
            CreateMap<DistrictAllocasionPlatform, DistrictAllocasionPlatformData>();
            CreateMap<DistrictAllocasionPlatformData, DistrictAllocasionPlatform>();
        }
    }
}
