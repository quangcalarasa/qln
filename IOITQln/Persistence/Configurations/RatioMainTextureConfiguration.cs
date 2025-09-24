using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class RatioMainTextureConfiguration : AppEntityTypeIntConfiguration<RatioMainTexture>
    {
        public override void Configure(EntityTypeBuilder<RatioMainTexture> builder)
        {
            base.Configure(builder);
            builder.ToTable("RatioMainTexture");

            builder.Property(c => c.ParentId).IsRequired(false);
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.TypeMainTexTure1).IsRequired(false);
            builder.Property(c => c.TypeMainTexTure2).IsRequired(false);
            builder.Property(c => c.TypeMainTexTure3).IsRequired(false);
            builder.Property(c => c.TypeMainTexTure4).IsRequired(false);
            builder.Property(c => c.TypeMainTexTure5).IsRequired(false);
            builder.Property(c => c.TypeMainTexTure6).IsRequired(false);
        }
    }
}
