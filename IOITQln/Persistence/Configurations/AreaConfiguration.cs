using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class AreaConfiguration : AppEntityTypeIntConfiguration<Area>
    {
        public override void Configure(EntityTypeBuilder<Area> builder)
        {
            base.Configure(builder);
            builder.ToTable("Area");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.FloorId).IsRequired();
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.IsMezzanine).IsRequired(false);
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
