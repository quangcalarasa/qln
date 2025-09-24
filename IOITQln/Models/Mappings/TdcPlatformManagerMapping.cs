using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcPlatformManagerMapping: Profile
    {
        public TdcPlatformManagerMapping()
        {
            CreateMap<TdcPlatformManagerData, TdcPlatformManager>();
            CreateMap<TdcPlatformManager, TdcPlatformManagerData>();
        }
    }
}
