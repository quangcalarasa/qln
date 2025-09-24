using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class CurrentStateMainTextureConfiguration : AppEntityTypeIntConfiguration<CurrentStateMainTexture>
    {
        public override void Configure(EntityTypeBuilder<CurrentStateMainTexture> builder)
        {
            base.Configure(builder);
            builder.ToTable("CurrentStateMainTexture");

            builder.Property(c => c.TypeMainTexTure).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Default).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
