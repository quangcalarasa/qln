using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class TDCProjectIngrePriceMapping : Profile
    {
        public TDCProjectIngrePriceMapping()
        {
            CreateMap<TDCProjectIngrePrice, TDCProjectIngrePriceData>();
            CreateMap<TDCProjectIngrePriceData, TDCProjectIngrePrice>();
        }
    }
}
