using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations 
{
    public class ChildDefaultCoefficientsConfigurations : AppEntityTypeIntConfiguration<ChildDefaultCoefficient>
    {
        public override void Configure(EntityTypeBuilder<ChildDefaultCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("ChildDefaultCoefficient");
        }
    }
}
