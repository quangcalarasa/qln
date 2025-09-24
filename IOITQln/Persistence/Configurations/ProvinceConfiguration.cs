using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ProvinceConfiguration : AppEntityTypeIntConfiguration<Province>
    {
        public override void Configure(EntityTypeBuilder<Province> builder)
        {
            base.Configure(builder);
            builder.ToTable("Province");

            builder.Property(c => c.Code).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
