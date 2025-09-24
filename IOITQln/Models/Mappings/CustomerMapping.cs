using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;

namespace IOITQln.Models.Mappings
{
    public class CustomerMapping : Profile
    {
        public CustomerMapping()
        {
            CreateMap<Customer, CustomerNocReportRes>();
        }
    }
}
