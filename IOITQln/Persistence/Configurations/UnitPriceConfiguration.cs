using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class UnitPriceConfiguration : AppEntityTypeIntConfiguration<UnitPrice>
    {
        public override void Configure(EntityTypeBuilder<UnitPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("UnitPrice");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
