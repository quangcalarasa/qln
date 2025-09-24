using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class RentFileBCTMapping : Profile
    {
        public RentFileBCTMapping()
        {
            CreateMap<RentFileBCT, RentFileBCTData>();
            CreateMap<RentFileBCTData, RentFileBCT>();
        }
    }
}
