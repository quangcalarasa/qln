using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcApartmentManagerMapping: Profile
    {
        public TdcApartmentManagerMapping()
        {
            CreateMap<TdcApartmentManagerData, TdcApartmentManager>();
            CreateMap<TdcApartmentManager, TdcApartmentManagerData>();
        }
    }
}
