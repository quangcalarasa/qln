using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingConstructionPriceConfiguration : AppEntityTypeLongConfiguration<PricingConstructionPrice>
    {
        public override void Configure(EntityTypeBuilder<PricingConstructionPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingConstructionPrice");
        }
    }
}
