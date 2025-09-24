using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167PostionValueConfiguration : AppEntityTypeIntConfiguration<Md167PositionValue>
    {
        public override void Configure(EntityTypeBuilder<Md167PositionValue> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167PositionValue");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Position1).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Position2).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Position3).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Position4).HasMaxLength(2000).IsRequired();
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
