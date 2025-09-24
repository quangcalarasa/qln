using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class WardManagementConfiguration : AppEntityTypeLongConfiguration<WardManagement>
    {
        public override void Configure(EntityTypeBuilder<WardManagement> builder)
        {
            base.Configure(builder);
            builder.ToTable("WardManagement");

            builder.Property(c => c.WardName).HasMaxLength(2000).IsRequired();
        }
    }
}
