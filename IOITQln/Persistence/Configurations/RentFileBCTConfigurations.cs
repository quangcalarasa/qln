using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class RentFileBCTConfigurations : AppEntityTypeIntConfiguration<RentFileBCT>
    {
        public override void Configure(EntityTypeBuilder<RentFileBCT> builder)
        {
            base.Configure(builder);
            builder.ToTable("RentFileBCT");

            builder.Property(c => c.Area).HasColumnType("decimal(18,2)");
        }
    }
}
