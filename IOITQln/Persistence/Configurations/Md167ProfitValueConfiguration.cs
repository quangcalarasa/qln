using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167ProfitValueConfiguration : AppEntityTypeIntConfiguration<Md167ProfitValue>
    {
        public override void Configure(EntityTypeBuilder<Md167ProfitValue> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167ProfitValue");

            builder.Property(c => c.UnitPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
