using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class SalaryCoefficientMapping : Profile
    {
        public SalaryCoefficientMapping()
        {
            CreateMap<SalaryCoefficient, SalaryCoefficientData>();
            CreateMap<SalaryCoefficientData, SalaryCoefficient>();
        }
    }
}
