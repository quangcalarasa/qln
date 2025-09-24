using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class DeductionLandMoneyMapping : Profile
    {
        public DeductionLandMoneyMapping()
        {
            CreateMap<DeductionLandMoney, DeductionLandMoneyData>();
            CreateMap<DeductionLandMoneyData, DeductionLandMoney>();
        }
    }
}
