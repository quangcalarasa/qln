using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ApartmentLandDetailConfiguration : AppEntityTypeLongConfiguration<ApartmentLandDetail>
    {
        public override void Configure(EntityTypeBuilder<ApartmentLandDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("ApartmentLandDetail");

        }
    }
}
