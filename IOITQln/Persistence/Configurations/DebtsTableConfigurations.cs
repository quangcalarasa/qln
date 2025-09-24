using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DebtsTableConfigurations : AppEntityTypeIntConfiguration<DebtsTable>
    {
        public override void Configure(EntityTypeBuilder<DebtsTable> builder)
        {
            base.Configure(builder);
            builder.ToTable("DebtsTable");

            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceDiff).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountExclude).HasColumnType("decimal(18,2)");
        }
    }
}
