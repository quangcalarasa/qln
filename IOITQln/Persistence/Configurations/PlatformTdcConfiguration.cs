using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PlatformTdcConfiguration: AppEntityTypeIntConfiguration<PlatformTdc>
    {
        public override void Configure(EntityTypeBuilder<PlatformTdc> builder)
        {
            base.Configure(builder);

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(6000).IsRequired();
            builder.Property(c => c.LandArea).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
