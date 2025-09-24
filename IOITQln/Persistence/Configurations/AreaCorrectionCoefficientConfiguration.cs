using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class AreaCorrectionCoefficientConfiguration : AppEntityTypeIntConfiguration<AreaCorrectionCoefficient>
    {
        public override void Configure(EntityTypeBuilder<AreaCorrectionCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("AreaCorrectionCoefficient");

            builder.Property(c => c.Des).HasMaxLength(4000);
            builder.Property(c => c.Name).HasMaxLength(2000);
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
