using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandConfiguration : AppEntityTypeIntConfiguration<Land>
    {
        public override void Configure(EntityTypeBuilder<Land> builder)
        {
            base.Configure(builder);
            builder.ToTable("Land");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(6000).IsRequired();
            builder.Property(c => c.TotalArea).IsRequired();
            builder.Property(c => c.ConstructionApartment).IsRequired();
            builder.Property(c => c.ConstructionLand).IsRequired();
            builder.Property(c => c.ConstructionValue).IsRequired();
            builder.Property(c => c.ContrustionBuild).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
