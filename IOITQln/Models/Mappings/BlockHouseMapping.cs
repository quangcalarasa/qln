using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class BlockHouseMapping : Profile
    {
        public BlockHouseMapping()
        {
            CreateMap<BlockHouse, BlockHouseData>();
            CreateMap<BlockHouseData, BlockData>();
        }
    }
}
