using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceRentPayConfigurations : AppEntityTypeIntConfiguration<TdcPriceRentPay>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceRentPay> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceRentPay");

            builder.Property(c => c.AmountPaid).HasColumnType("decimal(18,2)");
         
        }


    }
}
