using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class FloorBlockMapConfiguration : AppEntityTypeIntConfiguration<FloorBlockMap>
    {
        public override void Configure(EntityTypeBuilder<FloorBlockMap> builder)
        {
            base.Configure(builder);
            builder.ToTable("FloorBlockMap");

            builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.FloorId).IsRequired();
        }
    }
}
