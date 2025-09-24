using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167LandTaxConfiguration : AppEntityTypeIntConfiguration<Md167LandTax>
    {
        public override void Configure(EntityTypeBuilder<Md167LandTax> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167LandTax");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Code).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.IsDefault).IsRequired();
            builder.Property(c => c.TypeArea).HasMaxLength(2000);
            builder.Property(c => c.Tax).HasColumnType("decimal(18,5)").IsRequired();

            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
