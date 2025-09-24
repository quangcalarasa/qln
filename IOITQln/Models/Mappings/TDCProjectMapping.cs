using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCProjectMapping : Profile
    {
        public TDCProjectMapping()
        {
            CreateMap<TDCProject, TDCProjectData>();
            CreateMap<TDCProjectData, TDCProject>();
        }
    }
}
