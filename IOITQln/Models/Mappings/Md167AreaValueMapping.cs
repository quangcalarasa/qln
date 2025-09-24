using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class Md167AreaValueMapping : Profile
    {
        public Md167AreaValueMapping()
        {
            CreateMap<Md167AreaValue, Md167AreaValueData>();
            CreateMap<Md167AreaValueData, Md167AreaValue>();
        }
    }
}
