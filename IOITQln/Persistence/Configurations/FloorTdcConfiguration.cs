using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class FloorTdcConfiguration: AppEntityTypeIntConfiguration<FloorTdc>
    {
        public override void Configure(EntityTypeBuilder<FloorTdc> builder)
        {
            base.Configure(builder);
            builder.ToTable("FloorTdc");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(6000).IsRequired();
            builder.Property(c => c.ConstructionValue).IsRequired();
            builder.Property(c => c.ContrustionBuild).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
