using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class RatioMainTextureMapping : Profile
    {
        public RatioMainTextureMapping()
        {
            CreateMap<RatioMainTexture, RatioMainTextureData>();
            CreateMap<RatioMainTextureData, RatioMainTexture>();
        }
    }
}
