using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandPriceItemConfiguration : AppEntityTypeIntConfiguration<LandPriceItem>
    {
        public override void Configure(EntityTypeBuilder<LandPriceItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandPriceItem");

            builder.Property(c => c.LaneName).HasMaxLength(2000);
            builder.Property(c => c.LaneStartName).HasMaxLength(2000);
            builder.Property(c => c.LaneEndName).HasMaxLength(2000);
            builder.Property(c => c.Des).HasMaxLength(4000);
        }
    }
}
