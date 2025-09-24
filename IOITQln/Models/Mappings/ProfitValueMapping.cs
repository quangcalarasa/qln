using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class ProfitValueMapping : Profile
    {
        public ProfitValueMapping()
        {
            CreateMap<ProfitValue, ProfitValueData>();
            CreateMap<ProfitValueData, ProfitValue>();
        }
    }
}
