using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceOneSellOfficialConfiguration: AppEntityTypeIntConfiguration<TdcPriceOneSellOfficial>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceOneSellOfficial> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceOneSellOfficial");

            builder.Property(c => c.Area).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Total).HasColumnType("decimal(18,2)");            
        }
    }
}
