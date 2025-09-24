using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TypeBlockMapConfiguration : AppEntityTypeIntConfiguration<TypeBlockMap>
    {
        public override void Configure(EntityTypeBuilder<TypeBlockMap> builder)
        {
            base.Configure(builder);
            builder.ToTable("TypeBlockMap");

        }
    }
}
