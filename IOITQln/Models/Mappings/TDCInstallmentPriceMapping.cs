using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCInstallmentPriceMapping : Profile
    {
        public TDCInstallmentPriceMapping()
        {
            CreateMap<TDCInstallmentPrice, TDCInstallmentPriceData>();
            CreateMap<TDCInstallmentPriceData, TDCInstallmentPrice>();
        }
    }
}
