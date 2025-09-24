using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;
using static IOITQln.Models.Data.TdcPriceRentData;

namespace IOITQln.Models.Mappings
{
    public class TdcPriceRentExcelMetaMapping : Profile
    {
        public TdcPriceRentExcelMetaMapping()
        {
            CreateMap<TdcPriceRentExcelMeta, TdcPriceRentMetaData>();
            CreateMap<TdcPriceRentMetaData, TdcPriceRentExcelMeta>();
        }
    }
}
