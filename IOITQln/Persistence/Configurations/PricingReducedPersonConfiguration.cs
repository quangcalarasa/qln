using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingReducedPersonConfiguration : AppEntityTypeLongConfiguration<PricingReducedPerson>
    {
        public override void Configure(EntityTypeBuilder<PricingReducedPerson> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingReducedPerson");

            builder.Property(c => c.Salary).HasColumnType("decimal(18,2)");
        }
    }
}
