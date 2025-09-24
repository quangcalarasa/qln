using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class ApartmentTdcMapping:Profile
    {
        public ApartmentTdcMapping()
        {
            CreateMap<ApartmentTdc, ApartmentTdcData>();
            CreateMap<ApartmentTdcData, ApartmentTdc>();
        }
    }
}
