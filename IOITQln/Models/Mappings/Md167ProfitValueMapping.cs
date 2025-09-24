using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class Md167ProfitValueMapping : Profile
    {
        public Md167ProfitValueMapping()
        {
            CreateMap<Md167ProfitValue, Md167ProfitValueData>();
            CreateMap<Md167ProfitValueData, Md167ProfitValue>();
        }
    }
}
