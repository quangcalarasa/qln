using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCInstallmentOfficialDetailMapping : Profile
    {
        public TDCInstallmentOfficialDetailMapping()
        {
            CreateMap<TDCInstallmentOfficialDetail, TDCInstallmentOfficialDetailData>();
            CreateMap<TDCInstallmentOfficialDetailData, TDCInstallmentOfficialDetail>();
        }
    }
}
