using IOITQln.Common.Bases.Configurations;
using IOITQln.Controllers.ApiTdc;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCProjectPriceAndTaxConfiguration : AppEntityTypeIntConfiguration<TDCProjectPriceAndTax>
    {
        public override void Configure(EntityTypeBuilder<TDCProjectPriceAndTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCProjectPriceAndTax");

            builder.Property(c => c.TDCProjectId).IsRequired();
            builder.Property(c => c.PriceAndTaxId).IsRequired();
            builder.Property(c => c.Location).IsRequired();
        }
    }
}
