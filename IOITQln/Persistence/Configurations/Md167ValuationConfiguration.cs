using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167ValuationConfiguration : AppEntityTypeLongConfiguration<Md167Valuation>
    {
        public override void Configure(EntityTypeBuilder<Md167Valuation> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Valuation");

            builder.Property(c => c.UnitValuation).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Attactment).HasMaxLength(2000).IsRequired(false);

        }
    }
}
