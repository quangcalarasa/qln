using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DecreeMapConfiguration : AppEntityTypeLongConfiguration<DecreeMap>
    {
        public override void Configure(EntityTypeBuilder<DecreeMap> builder)
        {
            base.Configure(builder);
            builder.ToTable("DecreeMap");

        }
    }
}
