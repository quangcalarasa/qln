using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class PricingMapping : Profile
    {
        public PricingMapping()
        {
            CreateMap<Pricing, PricingData>();
            CreateMap<PricingData, Pricing>();
            CreateMap<PricingLandTbl, PricingLandTblData>();
            //CreateMap<ApartmentDetail, ApartmentDetailData>();
            //CreateMap<ApartmentDetailData, ApartmentDetail>();
        }
    }
}
