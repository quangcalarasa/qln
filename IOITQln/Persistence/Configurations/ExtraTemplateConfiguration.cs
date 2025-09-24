using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraTemplateConfiguration : AppEntityTypeIntConfiguration<ExtraTemplate>
    {
        public override void Configure(EntityTypeBuilder<ExtraTemplate> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraTemplate");

            builder.Property(c => c.Header).IsRequired();
            builder.Property(c => c.Content).HasMaxLength(20000).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
        }
    }
}
