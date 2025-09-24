using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCInstallmentTemporaryDetailMapping : Profile
    {
        public TDCInstallmentTemporaryDetailMapping()
        {
            CreateMap<TDCInstallmentTemporaryDetail, TDCInstallmentTemporaryDetailData>();
            CreateMap<TDCInstallmentTemporaryDetailData, TDCInstallmentTemporaryDetail>();
        }
    }
}
