using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingOfficerConfiguration : AppEntityTypeLongConfiguration<PricingOfficer>
    {
        public override void Configure(EntityTypeBuilder<PricingOfficer> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingOfficer");

            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Function).HasMaxLength(4000).IsRequired(false);
        }
    }
}
