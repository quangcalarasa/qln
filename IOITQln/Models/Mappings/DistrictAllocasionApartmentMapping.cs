using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class DistrictAllocasionApartmentMapping: Profile
    {
        public DistrictAllocasionApartmentMapping()
        {
            CreateMap<DistrictAllocasionApartment, DistrictAllocasionApartmentData>();
            CreateMap<DistrictAllocasionApartmentData, DistrictAllocasionApartment>();
        }
    }
}
