using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using IOITQln.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceRentDataExcelConfigurations : AppEntityTypeIntConfiguration<TdcPriceRentExcelData>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceRentExcelData> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceRentDataExcel");

            builder.Property(c => c.DailyInterestRate).HasColumnType("decimal(18,2)");
            builder.Property(c => c.UnitPay).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceEarnings).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PricePaymentPeriod).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Pay).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Paid).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceDifference).HasColumnType("decimal(18,2)");

        }

    }
}
