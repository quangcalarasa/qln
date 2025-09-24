using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ConversionConfigurations : AppEntityTypeIntConfiguration<Conversion>
    {
        public override void Configure(EntityTypeBuilder<Conversion> builder)
        {
            base.Configure(builder);
            builder.ToTable("ConversionCoefficient");

            builder.Property(c => c.Code).HasMaxLength(4000);
            builder.Property(c => c.CoefficientName).HasMaxLength(4000);
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
