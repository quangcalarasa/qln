using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PricingCustomerConfiguration : AppEntityTypeLongConfiguration<PricingCustomer>
    {
        public override void Configure(EntityTypeBuilder<PricingCustomer> builder)
        {
            base.Configure(builder);
            builder.ToTable("PricingCustomer");

        }
    }
}
