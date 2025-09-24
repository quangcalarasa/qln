using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;


namespace IOITQln.Models.Mappings
{
    public class RentFileMapping : Profile
    {
        public RentFileMapping()
        {
            CreateMap<RentFile, RentFlieData>();
            CreateMap<RentFlieData, RentFile>();
        }
    }
}
