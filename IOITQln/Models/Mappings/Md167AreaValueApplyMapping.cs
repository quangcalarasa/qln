using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class Md167AreaValueApplyMapping : Profile
    {
        public Md167AreaValueApplyMapping()
        {
            CreateMap<Md167AreaValueApply, Md167AreaValueApplyData>();
            CreateMap<Md167AreaValueApplyData, Md167AreaValueApply>();
        }
    }
}
