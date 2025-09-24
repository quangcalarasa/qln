using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingLandTblConfiguration : AppEntityTypeLongConfiguration<PricingLandTbl>
    {
        public override void Configure(EntityTypeBuilder<PricingLandTbl> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingLandTbl");

            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceInYear).HasColumnType("decimal(18,2)");
            builder.Property(c => c.RemainingPrice).HasColumnType("decimal(18,2)");
        }
    }
}
