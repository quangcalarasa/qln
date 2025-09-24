using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class No2LandPriceConfiguration : AppEntityTypeIntConfiguration<No2LandPrice>
    {
        public override void Configure(EntityTypeBuilder<No2LandPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("No2LandPrice");

            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
