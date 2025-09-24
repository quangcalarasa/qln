using IOITQln.Entities;
using IOITQln.Models.Dto;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class LogActionMapping : Profile
    {
        public LogActionMapping()
        {
            CreateMap<LogActionModel, LogAction>();
        }
    }
}
