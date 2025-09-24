using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceOneSellTaxConfiguration: AppEntityTypeIntConfiguration<TdcPriceOneSellTax>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceOneSellTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceOneSellTax");

            builder.Property(c => c.Total).HasColumnType("decimal(18,2)");
        }
    }
}
