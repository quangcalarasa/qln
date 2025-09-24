using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcPriceOneSellTemporaryMapping: Profile
    {
        public TdcPriceOneSellTemporaryMapping() 
        {
            CreateMap<TdcPriceOneSellTemporaryData, TdcPriceOneSellTemporary>();
            CreateMap<TdcPriceOneSellTemporary, TdcPriceOneSellTemporaryData>();

        }
    }
}
