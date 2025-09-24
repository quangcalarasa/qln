using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcPriceRentMapping : Profile
    {
        public TdcPriceRentMapping()
        {
            CreateMap<TdcPriceRentData, TdcPriceRent>();
            CreateMap<TdcPriceRent, TdcPriceRentData>();
            CreateMap<TdcPriceRentTemporary, TdcPriceRentTemporaryData>();
            CreateMap<TdcPriceRentTemporaryData, TdcPriceRentTemporary>();
            CreateMap<TdcPriceRentOfficial, TdcPriceRentOfficialData>();
            CreateMap<TdcPriceRentOfficialData, TdcPriceRentOfficial>();

        }
    }
}
