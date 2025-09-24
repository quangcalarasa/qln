using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167AreaValueConfiguration : AppEntityTypeIntConfiguration<Md167AreaValue>
    {
        public override void Configure(EntityTypeBuilder<Md167AreaValue> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167AreaValue");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Value).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.Decision).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.LandPurpose).HasMaxLength(2000).IsRequired();
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
