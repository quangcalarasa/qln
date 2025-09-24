using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class Md167HouseMapping : Profile
    {
        public Md167HouseMapping() 
        {
            CreateMap<Md167House, Md167HouseData>();
            CreateMap<Md167HouseData, Md167House>();
        }
    }
}
