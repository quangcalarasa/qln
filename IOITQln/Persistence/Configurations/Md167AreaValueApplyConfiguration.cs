using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167AreaValueApplyConfiguration : AppEntityTypeIntConfiguration<Md167AreaValueApply>
    {
        public override void Configure(EntityTypeBuilder<Md167AreaValueApply> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167AreaValueApply");

        }
    }
}
