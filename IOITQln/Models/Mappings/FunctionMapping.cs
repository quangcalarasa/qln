using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class FunctionMapping : Profile
    {
        public FunctionMapping()
        {
            CreateMap<Function, FunctionData>();
        }
    }
}
