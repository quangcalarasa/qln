using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167PricePerMonthConfiguration : AppEntityTypeLongConfiguration<Md167PricePerMonth>
    {
        public override void Configure(EntityTypeBuilder<Md167PricePerMonth> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167PricePerMonth");

            builder.Property(c => c.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.HousePrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.VatPrice).HasColumnType("decimal(18,2)");
        }
    }
}
