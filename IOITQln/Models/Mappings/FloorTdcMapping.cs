using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class FloorTdcMapping: Profile
    {
        public FloorTdcMapping()
        {
            CreateMap<FloorTdc, FloorTdcData>();
            CreateMap<FloorTdcData, FloorTdc>();
        }

    }
}
