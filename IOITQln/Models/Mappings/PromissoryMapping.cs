using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;
using IOITQln.Models.Dto;

namespace IOITQln.Models.Mappings
{
    public class PromissoryMapping : Profile
    {
        public PromissoryMapping()
        {
            CreateMap<NocReceipt, PromissoryReportRes>();
            CreateMap<PromissoryReportRes, NocReceipt>();
        }
    }
}
