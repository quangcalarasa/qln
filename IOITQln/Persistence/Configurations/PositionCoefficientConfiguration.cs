using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PositionCoefficientConfiguration : AppEntityTypeIntConfiguration<PositionCoefficient>
    {
        public override void Configure(EntityTypeBuilder<PositionCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("PositionCoefficient");

            builder.Property(c => c.Name).HasMaxLength(2000);
        }
    }
}
