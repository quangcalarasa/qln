using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167TranferUnitConfiguration : AppEntityTypeIntConfiguration<Md167TranferUnit>
    {
        public override void Configure(EntityTypeBuilder<Md167TranferUnit> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167TranferUnit");

            builder.Property(c => c.Code).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(2000).IsRequired();
        }
    }
}
