using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class InvestmentRateMapping : Profile
    {
        public InvestmentRateMapping()
        {
            CreateMap<InvestmentRate, InvestmentRateData>();
            CreateMap<InvestmentRateData, InvestmentRate>();
        }
    }
}
