using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class EditHistoryConfiguration : AppEntityTypeLongConfiguration<EditHistory>
    {
        public override void Configure(EntityTypeBuilder<EditHistory> builder)
        {
            base.Configure(builder);
            builder.ToTable("EditHistory");

            builder.Property(c => c.ContentUpdate).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.ReasonUpdate).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.AttactmentUpdate).HasMaxLength(500).IsRequired(false);
        }
    }
}
