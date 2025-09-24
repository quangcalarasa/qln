using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcInstallmentPricePayConfigurations : AppEntityTypeIntConfiguration<TDCInstallmentPricePay>
    {
        public override void Configure(EntityTypeBuilder<TDCInstallmentPricePay> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCInstallmentPricePay");

            builder.Property(c => c.Value).HasColumnType("decimal(18,2)");
         
        }


    }
}
