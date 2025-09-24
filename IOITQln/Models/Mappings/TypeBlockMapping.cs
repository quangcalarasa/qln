using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TypeBlockMapping : Profile
    {
        public TypeBlockMapping()
        {
            CreateMap<TypeBlock, TypeBlockData>();
            CreateMap<TypeBlockData, TypeBlock>();
        }
    }
}
