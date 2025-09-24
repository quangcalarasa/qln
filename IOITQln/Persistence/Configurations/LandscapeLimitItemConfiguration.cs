using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandscapeLimitItemConfiguration : AppEntityTypeIntConfiguration<LandscapeLimitItem>
    {
        public override void Configure(EntityTypeBuilder<LandscapeLimitItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandscapeLimitItem");

        }
    }
}
