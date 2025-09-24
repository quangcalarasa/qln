using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167HouseInfoConfiguration : AppEntityTypeIntConfiguration<Md167HouseInfo>
    {
        public override void Configure(EntityTypeBuilder<Md167HouseInfo> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167HouseInfo");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Date).IsRequired();
            builder.Property(c => c.Md167HouseId).IsRequired();
        }
    }
}
