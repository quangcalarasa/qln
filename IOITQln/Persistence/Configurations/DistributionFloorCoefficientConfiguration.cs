using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DistributionFloorCoefficientConfiguration : AppEntityTypeIntConfiguration<DistributionFloorCoefficient>
    {
        public override void Configure(EntityTypeBuilder<DistributionFloorCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("DistributionFloorCoefficient");

        }
    }
}
