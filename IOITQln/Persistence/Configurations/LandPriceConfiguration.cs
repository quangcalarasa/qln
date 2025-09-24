using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandPriceConfiguration : AppEntityTypeIntConfiguration<LandPrice>
    {
        public override void Configure(EntityTypeBuilder<LandPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandPrice");

            builder.Property(c => c.Des).HasMaxLength(4000);
        }
    }
}
