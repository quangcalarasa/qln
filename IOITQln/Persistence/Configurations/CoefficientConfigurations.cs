using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class CoefficientConfigurations : AppEntityTypeIntConfiguration<Coefficient>
    {
        public override void Configure(EntityTypeBuilder<Coefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("TimeCoefficient");

            builder.Property(c => c.UnitPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
