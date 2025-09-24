using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingApartmentLandDetailConfiguration : AppEntityTypeLongConfiguration<PricingApartmentLandDetail>
    {
        public override void Configure(EntityTypeBuilder<PricingApartmentLandDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingApartmentLandDetail");

            builder.Property(c => c.LandPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandUnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandPriceAfterReduced).HasColumnType("decimal(18,2)");
        }
    }
}
