using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ProfitValueConfiguration : AppEntityTypeIntConfiguration<ProfitValue>
    {
        public override void Configure(EntityTypeBuilder<ProfitValue> builder)
        {
            base.Configure(builder);
            builder.ToTable("ProfitValue");

            builder.Property(c => c.UnitPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
