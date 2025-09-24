using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class BlockHouseConfiguration : AppEntityTypeIntConfiguration<BlockHouse>
    {
        public override void Configure(EntityTypeBuilder<BlockHouse> builder)
        {
            base.Configure(builder);
            builder.ToTable("BlockHouse");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(6000).IsRequired();
            builder.Property(c => c.ConstructionValue).IsRequired();
            builder.Property(c => c.ContrustionBuild).IsRequired();
        }
    }
}
