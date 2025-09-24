using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcPriceOneSellOfficialMapping: Profile
    {
        public TdcPriceOneSellOfficialMapping()
        {
            CreateMap<TdcPriceOneSellOfficialData, TdcPriceOneSellOfficial>();
            CreateMap<TdcPriceOneSellOfficial, TdcPriceOneSellOfficialData>();
        }
    }
}
