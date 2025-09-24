using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ResettlementApartmentConfiguration : AppEntityTypeIntConfiguration<ResettlementApartment>
    {
        public override void Configure(EntityTypeBuilder<ResettlementApartment> builder)
        {
            base.Configure(builder);
            builder.ToTable("ResettlementApartment");

            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(1000).IsRequired();
        }
    }
}
