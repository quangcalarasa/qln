using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class BlockDetailConfiguration : AppEntityTypeLongConfiguration<BlockDetail>
    {
        public override void Configure(EntityTypeBuilder<BlockDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("BlockDetail");

        }
    }
}
