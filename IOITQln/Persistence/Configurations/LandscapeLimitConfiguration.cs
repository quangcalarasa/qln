using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LandscapeLimitConfiguration : AppEntityTypeIntConfiguration<LandscapeLimit>
    {
        public override void Configure(EntityTypeBuilder<LandscapeLimit> builder)
        {
            base.Configure(builder);
            builder.ToTable("LandscapeLimit");

            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
