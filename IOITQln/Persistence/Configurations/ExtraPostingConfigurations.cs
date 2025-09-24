
using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraPostingConfigurations : AppEntityTypeIntConfiguration<ExtraPostingConfiguration>
    {
        public override void Configure(EntityTypeBuilder<ExtraPostingConfiguration> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraPostingConfiguration");
        }
    }
}
