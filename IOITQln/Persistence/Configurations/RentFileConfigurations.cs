using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class RentFileConfigurations : AppEntityTypeGuidConfiguration<RentFile>
    {
        public override void Configure(EntityTypeBuilder<RentFile> builder)
        {
            base.Configure(builder);
            builder.ToTable("RentFile");
        }
    }
}
