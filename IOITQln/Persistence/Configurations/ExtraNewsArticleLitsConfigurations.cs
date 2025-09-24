
using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraNewsArticleLitsConfigurations : AppEntityTypeIntConfiguration<ExtraNewsArticleList>
    {
        public override void Configure(EntityTypeBuilder<ExtraNewsArticleList> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraNewsArticleList");
        }
    }
}
