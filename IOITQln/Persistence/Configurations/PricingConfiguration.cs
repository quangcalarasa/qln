using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingConfiguration : AppEntityTypeIntConfiguration<Pricing>
    {
        public override void Configure(EntityTypeBuilder<Pricing> builder)
        {
            base.Configure(builder);
            builder.ToTable("Pricing");

            builder.Property(c => c.TimeUse).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.ApartmentPriceReducedNote).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.ApartmentPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApartmentPriceNoVat).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApartmentPriceReduced).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApartmentPriceRemaining).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApartmentPriceVat).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandPriceAfterReduced).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ProcessProfileCeCode).HasMaxLength(1000).IsRequired(false);
        }
    }
}
