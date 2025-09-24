using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcPriceOneSellMapping: Profile
    {
        public TdcPriceOneSellMapping()
        {
            CreateMap<TdcPriceOneSellData, TdcPriceOneSell>();
            CreateMap<TdcPriceOneSell, TdcPriceOneSellData>();
        }
    }
}
