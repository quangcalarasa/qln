using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ChilDfTableConfigurations : AppEntityTypeIntConfiguration<ChilDfTable>
    {
        public override void Configure(EntityTypeBuilder<ChilDfTable> builder)
        {
            base.Configure(builder);
            builder.ToTable("ChilDfTable");

            builder.Property(c => c.Value).HasColumnType("decimal(18,2)");
        }
    }
}
