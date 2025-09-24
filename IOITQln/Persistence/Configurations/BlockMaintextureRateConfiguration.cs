using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class BlockMaintextureRateConfiguration : AppEntityTypeLongConfiguration<BlockMaintextureRate>
    {
        public override void Configure(EntityTypeBuilder<BlockMaintextureRate> builder)
        {
            base.Configure(builder);
            builder.ToTable("BlockMaintextureRate");

        }
    }
}
