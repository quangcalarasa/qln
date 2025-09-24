using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TypeAttributeMapping : Profile
    {
        public TypeAttributeMapping()
        {
            CreateMap<TypeAttribute, TypeAttributeData>();
        }
    }
}
