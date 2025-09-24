using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class QuickMathLogConfigurations : AppEntityTypeIntConfiguration<QuickMathLog>
    {
        public override void Configure(EntityTypeBuilder<QuickMathLog> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuickMathLog");
        }
    }
}
