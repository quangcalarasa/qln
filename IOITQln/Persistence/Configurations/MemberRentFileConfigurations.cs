using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class MemberRentFileConfigurations : AppEntityTypeIntConfiguration<MemberRentFile>
    {
        public override void Configure(EntityTypeBuilder<MemberRentFile> builder)
        {
            base.Configure(builder);
            builder.ToTable("MemberRentFile");

        }
    }
}
