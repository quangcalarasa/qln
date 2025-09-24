using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DefaultCoefficientConfigurations : AppEntityTypeIntConfiguration<DefaultCoefficient>
    {
        public override void Configure(EntityTypeBuilder<DefaultCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("DefaultCoefficient");

            builder.Property(c => c.UnitPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
            builder.Property(c => c.Content).HasMaxLength(4000);
            builder.Property(c => c.CoefficientName).HasMaxLength(4000);

        }
    }
}
