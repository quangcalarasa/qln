using IOITQln.Common.Bases.Configurations;
using IOITQln.Controllers.ApiTdc;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class OriginalPriceAndTaxConfiguration : AppEntityTypeIntConfiguration<OriginalPriceAndTax>
    {
        public override void Configure(EntityTypeBuilder<OriginalPriceAndTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("OriginalPriceAndTax");
            
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
