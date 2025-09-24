using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandCorrectionCoefficientConfiguration : AppEntityTypeIntConfiguration<LandPriceCorrectionCoefficient>
    {
        public override void Configure(EntityTypeBuilder<LandPriceCorrectionCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandCorrectionCoefficient");

            builder.Property(c => c.Code).HasMaxLength(1000);
            builder.Property(c => c.Name).HasMaxLength(2000);
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
