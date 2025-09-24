using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;
using static IOITQln.Models.Data.TdcInstallmentPriceTable;

namespace IOITQln.Models.Mappings
{
    public class InstallmentPriceTableMetaTdcMapping : Profile
    {
        public InstallmentPriceTableMetaTdcMapping()
        {
            CreateMap<InstallmentPriceTableMetaTdc, InstallmentPriceTableMetaTdcData>();
            CreateMap<InstallmentPriceTableMetaTdcData, InstallmentPriceTableMetaTdc>();
        }
    }
}
