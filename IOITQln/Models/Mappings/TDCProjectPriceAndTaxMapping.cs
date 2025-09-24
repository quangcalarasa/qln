using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCProjectPriceAndTaxMapping : Profile
    {
        public TDCProjectPriceAndTaxMapping()
        {
            CreateMap<TDCProjectPriceAndTax, TDCProjectPriceAndTaxData>();
            CreateMap<TDCProjectPriceAndTaxData, TDCProjectPriceAndTax>();
        }
    }
}
