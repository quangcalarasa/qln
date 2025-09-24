using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceRentTaxConfigurations : AppEntityTypeIntConfiguration<TdcPriceRentTax>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceRentTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceRentTax");

            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
        }
    }
}
