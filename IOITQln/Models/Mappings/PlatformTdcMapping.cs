using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class PlatformTdcMapping:Profile
    {
        public PlatformTdcMapping()
        {
            CreateMap<PlatformTdc, PlatformTdcData>();
            CreateMap<PlatformTdcData, PlatformTdc>();
        }
    }
}
