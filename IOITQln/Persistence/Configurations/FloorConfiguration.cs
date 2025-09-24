using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class FloorConfiguration : AppEntityTypeIntConfiguration<Floor>
    {
        public override void Configure(EntityTypeBuilder<Floor> builder)
        {
            base.Configure(builder);
            builder.ToTable("Floor");

            builder.Property(c => c.Code).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
        }
    }
}
