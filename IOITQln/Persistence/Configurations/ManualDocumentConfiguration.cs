using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ManualDocumentConfiguration : AppEntityTypeIntConfiguration<ManualDocument>
    {
        public override void Configure(EntityTypeBuilder<ManualDocument> builder)
        {
            base.Configure(builder);
            builder.ToTable("ManualDocument");

            builder.Property(c => c.Title).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Attactment).HasMaxLength(2000).IsRequired(false);
        }
    }
}
