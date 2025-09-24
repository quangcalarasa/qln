using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class ExtraEmailDebtMapping : Profile
    {
        public ExtraEmailDebtMapping()
        {
            CreateMap<ExtraEmailDebt, ExtraEmailDebtData>();
            CreateMap<ExtraEmailDebtData, ExtraEmailDebt>();
        }
    }
}
