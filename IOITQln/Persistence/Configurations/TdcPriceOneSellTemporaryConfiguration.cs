using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceOneSellTemporaryConfiguration: AppEntityTypeIntConfiguration<TdcPriceOneSellTemporary>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceOneSellTemporary> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceOneSellTemporary");

            builder.Property(c => c.Area).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Total).HasColumnType("decimal(18,2)");
        }
    }
}
