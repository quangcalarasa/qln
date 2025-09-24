using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class AnnualInstallmentMapping :Profile
    {
        public AnnualInstallmentMapping()
        {
            CreateMap<AnnualInstallment, AnnualInstallmentData>();
            CreateMap<AnnualInstallmentData, AnnualInstallment>();
        }
    }
}
