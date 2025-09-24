using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class FunctionRoleConfiguration : AppEntityTypeIntConfiguration<FunctionRole>
    {
        public override void Configure(EntityTypeBuilder<FunctionRole> builder)
        {
            base.Configure(builder);
            builder.ToTable("FunctionRole");

            builder.Property(c => c.ActiveKey).HasMaxLength(10).IsRequired(true);
        }
    }
}
