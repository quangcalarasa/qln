using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class ApartmentMapping : Profile
    {
        public ApartmentMapping()
        {
            CreateMap<Apartment, ApartmentData>();
            CreateMap<ApartmentData, Apartment>();
            CreateMap<ApartmentDetail, ApartmentDetailData>();
            CreateMap<ApartmentDetailData, ApartmentDetail>();
        }
    }
}
