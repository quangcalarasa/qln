using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class WardConfiguration : AppEntityTypeIntConfiguration<Ward>
    {
        public override void Configure(EntityTypeBuilder<Ward> builder)
        {
            base.Configure(builder);
            builder.ToTable("Ward");

            builder.Property(c => c.Code).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.OldName).HasColumnType("nvarchar(MAX)");
        }
    }
}
