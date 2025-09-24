using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PriceListItemConfiguration : AppEntityTypeIntConfiguration<PriceListItem>
    {
        public override void Configure(EntityTypeBuilder<PriceListItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("PriceListItem");

            builder.Property(c => c.DetailStructure).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.NameOfConstruction).HasMaxLength(2000).IsRequired();
        }
    }
}
