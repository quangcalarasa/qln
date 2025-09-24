using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TemplateConfiguration : AppEntityTypeIntConfiguration<Template>
    {
        public override void Configure(EntityTypeBuilder<Template> builder)
        {
            base.Configure(builder);
            builder.ToTable("Template");

            builder.Property(c => c.Code).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.ParentName).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Attactment).HasMaxLength(2000).IsRequired(false);
        }
    }
}
