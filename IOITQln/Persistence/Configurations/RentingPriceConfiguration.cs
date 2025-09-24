using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class RentingPriceConfiguration : AppEntityTypeIntConfiguration<RentingPrice>
    {
        public override void Configure(EntityTypeBuilder<RentingPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("RentingPrice");

            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
