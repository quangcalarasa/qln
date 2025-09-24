using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class salaryMapping : Profile
    {
        public salaryMapping()
        {
            CreateMap<Salary, SalaryData>();
            CreateMap<SalaryData, Salary>();
        }
    }
}
