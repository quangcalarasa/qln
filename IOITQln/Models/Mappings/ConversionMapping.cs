using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class ConversionMapping : Profile
    {
        public ConversionMapping()
        {
            CreateMap<Conversion, ConversionData>();
            CreateMap<ConversionData, Conversion>();
        }
    }
}
