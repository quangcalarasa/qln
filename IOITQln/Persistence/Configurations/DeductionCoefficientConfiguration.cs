using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DeductionCoefficientConfiguration : AppEntityTypeIntConfiguration<DeductionCoefficient>
    {
        public override void Configure(EntityTypeBuilder<DeductionCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("DeductionCoefficient");

            builder.Property(c => c.ObjectApply).HasMaxLength(2000);
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
