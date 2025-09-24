using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandSpecialCoefficientConfiguration : AppEntityTypeIntConfiguration<LandSpecialCoefficient>
    {
        public override void Configure(EntityTypeBuilder<LandSpecialCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandSpecialCoefficient");
        }
    }
}
