using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCInstallmentTemporaryDetailConfiguration : AppEntityTypeIntConfiguration<TDCInstallmentTemporaryDetail>
    {
        public override void Configure(EntityTypeBuilder<TDCInstallmentTemporaryDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCInstallmentTemporaryDetail");

            builder.Property(c => c.Area).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.UnitPrice).HasColumnType("decimal(18,2)");

        }
    }
}
