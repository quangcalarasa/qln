using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LevelBlockMapConfiguration : AppEntityTypeIntConfiguration<LevelBlockMap>
    {
        public override void Configure(EntityTypeBuilder<LevelBlockMap> builder)
        {
            base.Configure(builder);
            builder.ToTable("LevelBlockMap");

            builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.LevelId).IsRequired();
        }
    }
}
