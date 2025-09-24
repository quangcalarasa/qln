using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCProjectPriceAndTaxDetailsMapping : Profile
    {
        public TDCProjectPriceAndTaxDetailsMapping()
        {
            CreateMap<TDCProjectPriceAndTaxDetails, TDCProjectPriceAndTaxDetailData>();
            CreateMap<TDCProjectPriceAndTaxDetailData, TDCProjectPriceAndTaxDetails>();
        }
    }
}
