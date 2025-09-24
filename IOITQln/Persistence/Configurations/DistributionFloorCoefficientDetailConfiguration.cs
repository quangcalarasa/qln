using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DistributionFloorCoefficientDetailConfiguration : AppEntityTypeIntConfiguration<DistributionFloorCoefficientDetail>
    {
        public override void Configure(EntityTypeBuilder<DistributionFloorCoefficientDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("DistributionFloorCoefficientDetail");

        }
    }
}
