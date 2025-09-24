using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class InstallmentPriceTableMetaTdcConfiguration : AppEntityTypeIntConfiguration<InstallmentPriceTableMetaTdc>
    {
        public override void Configure(EntityTypeBuilder<InstallmentPriceTableMetaTdc> builder)
        {
            base.Configure(builder);
            builder.ToTable("InstallmentPriceTableMetaTdc");

            builder.Property(c => c.Pay).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.Paid).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.PriceDifference).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.PayTimeId).IsRequired(false);

        }
    }
}
