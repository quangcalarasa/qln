using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ProcessProfileCeConfiguration : AppEntityTypeLongConfiguration<ProcessProfileCe>
    {
        public override void Configure(EntityTypeBuilder<ProcessProfileCe> builder)
        {
            base.Configure(builder);
            builder.ToTable("ProcessProfileCe");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(true);
            builder.Property(c => c.IdServiceRecord).HasMaxLength(500).IsRequired(true);
            builder.Property(c => c.CodeIdentify).HasMaxLength(2000).IsRequired(true);
        }
    }
}
