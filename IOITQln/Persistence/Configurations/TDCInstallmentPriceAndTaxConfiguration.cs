using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCInstallmentPriceAndTaxConfiguration : AppEntityTypeIntConfiguration<TDCInstallmentPriceAndTax>
    {
        public override void Configure(EntityTypeBuilder<TDCInstallmentPriceAndTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCInstallmentPriceAndTax");

            builder.Property(c => c.Value).HasColumnType("decimal(18,2)");
        }
    }
}
