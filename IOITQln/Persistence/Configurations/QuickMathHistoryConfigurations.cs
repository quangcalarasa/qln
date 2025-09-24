using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class QuickMathHistoryConfigurations : AppEntityTypeIntConfiguration<QuickMathHistory>
    {
        public override void Configure(EntityTypeBuilder<QuickMathHistory> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuickMathHistory");
 
        }
    }
}
