using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class Md167ContractMapping : Profile
    {
        public Md167ContractMapping()
        {
            CreateMap<Md167Contract, Md167ContractData>();
            CreateMap<Md167ContractData, Md167Contract>();
        }
    }
}
