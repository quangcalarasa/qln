using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraNewsArticlesConfigurations : AppEntityTypeIntConfiguration<ExtraNewsArticle>
    {
        public override void Configure(EntityTypeBuilder<ExtraNewsArticle> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraNewsArticle");

            builder.Property(c => c.ArticleTitle).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Content).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Files).HasMaxLength(4000).IsRequired(false);
        }
    }
}
