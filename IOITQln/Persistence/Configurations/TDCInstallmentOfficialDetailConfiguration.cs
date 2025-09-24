using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCInstallmentOfficialDetailConfiguration : AppEntityTypeIntConfiguration<TDCInstallmentOfficialDetail>
    {
        public override void Configure(EntityTypeBuilder<TDCInstallmentOfficialDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCInstallmentOfficialDetail");

            builder.Property(c => c.Area).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.UnitPrice).HasColumnType("decimal(18,2)");

        }
    }
}
