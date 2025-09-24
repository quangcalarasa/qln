using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class BlockMapping : Profile
    {
        public BlockMapping()
        {
            CreateMap<Block, BlockData>();
            CreateMap<BlockData, Block>();
            CreateMap<BlockDetail, BlockDetailData>();
            CreateMap<BlockDetailData, BlockDetail>();
        }
    }
}
