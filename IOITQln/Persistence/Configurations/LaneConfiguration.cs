using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class LaneConfiguration : AppEntityTypeIntConfiguration<Lane>
    {
        public override void Configure(EntityTypeBuilder<Lane> builder)
        {
            base.Configure(builder);
            builder.ToTable("Lane");

            builder.Property(c => c.Code).HasMaxLength(4000);
            builder.Property(c => c.Name).HasMaxLength(4000);
            builder.Property(c => c.OldNameLane).HasColumnType("ntext");
        }
    }
}
