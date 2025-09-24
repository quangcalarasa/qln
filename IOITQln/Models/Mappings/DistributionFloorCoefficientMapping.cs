using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class DistributionFloorCoefficientMapping : Profile
    {
        public DistributionFloorCoefficientMapping()
        {
            CreateMap<DistributionFloorCoefficient, DistributionFloorCoefficientData>();
            CreateMap<DistributionFloorCoefficientData, DistributionFloorCoefficient>();
        }
    }
}
