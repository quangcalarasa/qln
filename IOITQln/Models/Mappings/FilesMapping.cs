using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class FilesMapping : Profile
    {
        public FilesMapping()
        {
            CreateMap<Files, FilesData>();
            CreateMap<FilesData, Files>();
        }
    }
}
