using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ApartmentDetailConfiguration : AppEntityTypeLongConfiguration<ApartmentDetail>
    {
        public override void Configure(EntityTypeBuilder<ApartmentDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("ApartmentDetail");

            builder.Property(c => c.TargetId).IsRequired();
            builder.Property(c => c.Level).IsRequired(false);
            builder.Property(c => c.AreaId).IsRequired();
            builder.Property(c => c.FloorId).IsRequired();
            builder.Property(c => c.GeneralArea).IsRequired(false);
            builder.Property(c => c.PrivateArea).IsRequired(false);
        }
    }
}
